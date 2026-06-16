
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

public class StockController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public StockController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: STOCKS
    public async Task<IActionResult> Index() {
        return View(await _context.Stocks.ToListAsync());
    }
    // GET: STOCKS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var stock = await _context.Stocks
            .FirstOrDefaultAsync(m => m.Id == id);
        if (stock == null) {
            return NotFound();
        }

        return View(stock);
    }
    // GET: STOCKS/Create
    public IActionResult Create() {
        return View();
    }
    // POST: STOCKS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Product,ProductRef,LocationId,Location,Quantity,LastReceiptDate,CreatedAt,UpdatedAt")] Stock stock) {
        if (ModelState.IsValid) {
            _context.Add(stock);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(stock);
    }
    // GET: STOCKS/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var stock = await _context.Stocks.FindAsync(id);
        if (stock == null) {
            return NotFound();
        }
        return View(stock);
    }
    // POST: STOCKS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Product,ProductRef,LocationId,Location,Quantity,LastReceiptDate,CreatedAt,UpdatedAt")] Stock stock) {
        if (id != stock.Id) {
            return NotFound();
        }

        if (ModelState.IsValid) {
            try {
                _context.Update(stock);
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!StockExists(stock.Id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(stock);
    }
    // GET: STOCKS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var stock = await _context.Stocks
            .FirstOrDefaultAsync(m => m.Id == id);
        if (stock == null) {
            return NotFound();
        }

        return View(stock);
    }
    // POST: STOCKS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var stock = await _context.Stocks.FindAsync(id);
        if (stock != null) {
            _context.Stocks.Remove(stock);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    private bool StockExists(int? id) {
        return _context.Stocks.Any(e => e.Id == id);
    }

}
