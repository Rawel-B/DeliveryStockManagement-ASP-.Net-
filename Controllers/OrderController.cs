
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

public class OrderController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public OrderController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: ORDERS
    public async Task<IActionResult> Index() {
        return View(await _context.Orders.ToListAsync());
    }

    // GET: ORDERS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(m => m.Id == id);
        if (order == null) {
            return NotFound();
        }

        return View(order);
    }

    // GET: ORDERS/Create
    public IActionResult Create() {
        return View();
    }

    // POST: ORDERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,CustomerId,SupplierId,OrderDate,Status,TotalAmount,OrderNumber,Remark,Products,ShippingIds,InvoiceIds,CreatedAt,UpdatedAt")] Order order) {
        if (ModelState.IsValid) {
            _context.Add(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(order);
    }

    // GET: ORDERS/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var order = await _context.Orders.FindAsync(id);
        if (order == null) {
            return NotFound();
        }
        return View(order);
    }

    // POST: ORDERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,CustomerId,SupplierId,OrderDate,Status,TotalAmount,OrderNumber,Remark,Products,ShippingIds,InvoiceIds,CreatedAt,UpdatedAt")] Order order) {
        if (id != order.Id) {
            return NotFound();
        }

        if (ModelState.IsValid) {
            try {
                _context.Update(order);
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!OrderExists(order.Id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(order);
    }

    // GET: ORDERS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var order = await _context.Orders
            .FirstOrDefaultAsync(m => m.Id == id);
        if (order == null) {
            return NotFound();
        }

        return View(order);
    }

    // POST: ORDERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var order = await _context.Orders.FindAsync(id);
        if (order != null) {
            _context.Orders.Remove(order);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool OrderExists(int? id) {
        return _context.Orders.Any(e => e.Id == id);
    }
}
