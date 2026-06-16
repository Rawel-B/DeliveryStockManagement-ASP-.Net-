
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

public class ShippingController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public ShippingController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: SHIPPINGS
    public async Task<IActionResult> Index() {
        return View(await _context.Shippings.ToListAsync());
    }

    // GET: SHIPPINGS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var shipping = await _context.Shippings
            .FirstOrDefaultAsync(m => m.Id == id);
        if (shipping == null) {
            return NotFound();
        }

        return View(shipping);
    }

    // GET: SHIPPINGS/Create
    public IActionResult Create() {
        return View();
    }

    // POST: SHIPPINGS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,OrderId,CarrierId,DeliveryDate,ReceiptDate,Cost,Status,ShippingAddress,TrackingNumber,Remark,CreatedAt,UpdatedAt")] Shipping shipping) {
        if (ModelState.IsValid) {
            _context.Add(shipping);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(shipping);
    }

    // GET: SHIPPINGS/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var shipping = await _context.Shippings.FindAsync(id);
        if (shipping == null) {
            return NotFound();
        }
        return View(shipping);
    }

    // POST: SHIPPINGS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,OrderId,CarrierId,DeliveryDate,ReceiptDate,Cost,Status,ShippingAddress,TrackingNumber,Remark,CreatedAt,UpdatedAt")] Shipping shipping) {
        if (id != shipping.Id) {
            return NotFound();
        }

        if (ModelState.IsValid) {
            try {
                _context.Update(shipping);
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!ShippingExists(shipping.Id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(shipping);
    }

    // GET: SHIPPINGS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var shipping = await _context.Shippings
            .FirstOrDefaultAsync(m => m.Id == id);
        if (shipping == null) {
            return NotFound();
        }

        return View(shipping);
    }

    // POST: SHIPPINGS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var shipping = await _context.Shippings.FindAsync(id);
        if (shipping != null) {
            _context.Shippings.Remove(shipping);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ShippingExists(int? id) {
        return _context.Shippings.Any(e => e.Id == id);
    }
}
