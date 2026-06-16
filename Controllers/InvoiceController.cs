
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

public class InvoiceController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public InvoiceController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: INVOICES
    public async Task<IActionResult> Index() {
        return View(await _context.Invoices.ToListAsync());
    }

    // GET: INVOICES/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(m => m.Id == id);
        if (invoice == null) {
            return NotFound();
        }

        return View(invoice);
    }

    // GET: INVOICES/Create
    public IActionResult Create() {
        return View();
    }

    // POST: INVOICES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,OrderId,InvoicingDate,Status,Method,Amount,TransactionRef,Remark,CreatedAt,UpdatedAt")] Invoice invoice) {
        if (ModelState.IsValid) {
            _context.Add(invoice);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
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
        return View(invoice);
    }

    // GET: INVOICES/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(m => m.Id == id);
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
            _context.Invoices.Remove(invoice);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool InvoiceExists(int? id) {
        return _context.Invoices.Any(e => e.Id == id);
    }
}
