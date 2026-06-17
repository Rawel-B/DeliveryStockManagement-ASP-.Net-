using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

[Authorize(Roles = "administrator,manager")]
public class InvoiceController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public InvoiceController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: INVOICES
    public async Task<IActionResult> Index() {
        var invoices = await _context.Invoices.Include(i => i.Order).OrderByDescending(i => i.InvoicingDate).ToListAsync();
        foreach (var invoice in invoices) {
            invoice.OrderNumber = invoice.Order?.OrderNumber;
        }
        return View(invoices);
    }

    // GET: INVOICES/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var invoice = await _context.Invoices.Include(i => i.Order).FirstOrDefaultAsync(m => m.Id == id);
        if (invoice == null) {
            return NotFound();
        }
        invoice.OrderNumber = invoice.Order?.OrderNumber;
        return View(invoice);
    }

    // GET: INVOICES/Create
    public async Task<IActionResult> Create() {
        await LoadLookups();
        return View(new Invoice { InvoicingDate = DateTime.Now, Status = InvoiceStatus.pending });
    }

    // POST: INVOICES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,OrderId,InvoicingDate,Status,Method,Amount,TransactionRef,Remark,CreatedAt,UpdatedAt")] Invoice invoice) {
        var order = await _context.Orders.Include(o => o.Invoices).FirstOrDefaultAsync(o => o.Id == invoice.OrderId);
        if (order == null) {
            ModelState.AddModelError(nameof(invoice.OrderId), "Order Was Not Found.");
        } else {
            if (order.Status == OrderStatus.cancelled) ModelState.AddModelError(nameof(invoice.OrderId), "Cannot Invoice A Cancelled Order.");
            if (order.Status == OrderStatus.pendingApproval) ModelState.AddModelError(nameof(invoice.OrderId), "Only Validated, Ongoing, Or Delivered Orders Can Be Invoiced.");
            decimal covered = order.Invoices.Where(i => i.Status == InvoiceStatus.completed || i.Status == InvoiceStatus.processing || i.Status == InvoiceStatus.pending).Sum(i => i.Amount);
            if (covered >= order.TotalAmount) ModelState.AddModelError(nameof(invoice.Amount), "The Invoicing For This Order Is Already Covered.");
        }

        if (ModelState.IsValid) {
            invoice.Status = InvoiceStatus.pending;
            invoice.InvoicingDate = DateTime.Now;
            invoice.TransactionRef = string.IsNullOrWhiteSpace(invoice.TransactionRef) ? "TXN-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant() : invoice.TransactionRef.Trim();
            invoice.Remark = DsmControllerUtilities.CleanNullable(invoice.Remark);
            DsmControllerUtilities.StampNew(invoice);
            _context.Add(invoice);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await LoadLookups();
        return View(invoice);
    }

    // GET: INVOICES/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null) {
            return NotFound();
        }
        await LoadLookups();
        return View(invoice);
    }

    // POST: INVOICES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,OrderId,InvoicingDate,Status,Method,Amount,TransactionRef,Remark,CreatedAt,UpdatedAt")] Invoice invoice) {
        if (id != invoice.Id) {
            return NotFound();
        }
        if (ModelState.IsValid) {
            try {
                invoice.Remark = DsmControllerUtilities.CleanNullable(invoice.Remark);
                DsmControllerUtilities.StampUpdate(invoice);
                _context.Update(invoice);
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!InvoiceExists(invoice.Id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        await LoadLookups();
        return View(invoice);
    }

    // GET: INVOICES/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var invoice = await _context.Invoices.Include(i => i.Order).FirstOrDefaultAsync(m => m.Id == id);
        if (invoice == null) {
            return NotFound();
        }
        return View(invoice);
    }

    // POST: INVOICES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice != null) {
            if (invoice.Status == InvoiceStatus.completed) {
                ModelState.AddModelError(string.Empty, "Cannot Remove Completed Invoices.");
                return View("Delete", invoice);
            }
            _context.Invoices.Remove(invoice);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetInvoiceStatus(int id, InvoiceStatus status) {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null) {
            return NotFound();
        }
        string? error = ValidateStatusChange(invoice.Status, status);
        if (error != null) {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Details), new { id });
        }
        invoice.Status = status;
        DsmControllerUtilities.StampUpdate(invoice);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadLookups() {
        ViewBag.Orders = new SelectList(await _context.Orders.Where(o => o.Status != OrderStatus.pendingApproval && o.Status != OrderStatus.cancelled).OrderByDescending(o => o.OrderDate).ToListAsync(), "Id", "OrderNumber");
    }

    private string? ValidateStatusChange(InvoiceStatus current, InvoiceStatus next) {
        if (current == next) return null;
        bool allowed = current switch {
            InvoiceStatus.pending => next == InvoiceStatus.processing || next == InvoiceStatus.cancelled,
            InvoiceStatus.processing => next == InvoiceStatus.completed || next == InvoiceStatus.failed || next == InvoiceStatus.cancelled,
            InvoiceStatus.failed => next == InvoiceStatus.processing || next == InvoiceStatus.cancelled,
            InvoiceStatus.completed => next == InvoiceStatus.refunded,
            InvoiceStatus.refunded or InvoiceStatus.cancelled => false,
            _ => false
        };
        return allowed ? null : "This Invoice Cannot Move To That Status.";
    }

    private bool InvoiceExists(int? id) {
        return _context.Invoices.Any(e => e.Id == id);
    }
}
