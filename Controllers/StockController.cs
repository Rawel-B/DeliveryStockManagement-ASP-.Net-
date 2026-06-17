using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

[Authorize]
public class StockController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public StockController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: STOCKS
    public async Task<IActionResult> Index(string? criteria) {
        var stocks = _context.Stocks.AsQueryable();
        if (!string.IsNullOrWhiteSpace(criteria)) {
            string value = criteria.Trim();
            stocks = stocks.Where(s => s.Product.Contains(value) || (s.ProductRef != null && s.ProductRef.Contains(value)) || (s.Location != null && s.Location.Contains(value)));
        }
        var list = await stocks.OrderBy(s => s.Product).ToListAsync();
        ApplyStockQuantities(list);
        ViewBag.Criteria = criteria;
        return View(list);
    }

    // GET: STOCKS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var stock = await _context.Stocks.FirstOrDefaultAsync(m => m.Id == id);
        if (stock == null) {
            return NotFound();
        }
        ApplyStockQuantities(new List<Stock> { stock });
        return View(stock);
    }

    // GET: STOCKS/Create
    public async Task<IActionResult> Create() {
        await LoadLookups();
        return View();
    }

    // POST: STOCKS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Product,ProductRef,LocationId,Location,Quantity,LastReceiptDate,CreatedAt,UpdatedAt")] Stock stock) {
        stock.Product = DsmControllerUtilities.Clean(stock.Product);
        stock.ProductRef = DsmControllerUtilities.Clean(stock.ProductRef);
        if (string.IsNullOrWhiteSpace(stock.ProductRef)) {
            ModelState.AddModelError(nameof(stock.ProductRef), "product reference must be filled.");
        }
        if (stock.LocationId == null) {
            ModelState.AddModelError(nameof(stock.LocationId), "location must be specified.");
        }
        if (!string.IsNullOrWhiteSpace(stock.ProductRef) && await _context.Stocks.AnyAsync(s => s.ProductRef == stock.ProductRef)) {
            ModelState.AddModelError(nameof(stock.ProductRef), "Stock With This Product Reference Already Exists.");
        }
        await FillLocation(stock);
        if (ModelState.IsValid) {
            stock.Quantity = Math.Max(stock.Quantity, 0);
            if (stock.LastReceiptDate == default) {
                stock.LastReceiptDate = DateTime.Now;
            }
            DsmControllerUtilities.StampNew(stock);
            _context.Add(stock);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await LoadLookups();
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
        await LoadLookups();
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

        stock.Product = DsmControllerUtilities.Clean(stock.Product);
        stock.ProductRef = DsmControllerUtilities.Clean(stock.ProductRef);
        if (string.IsNullOrWhiteSpace(stock.ProductRef)) {
            ModelState.AddModelError(nameof(stock.ProductRef), "product reference must be filled.");
        }
        if (stock.LocationId == null) {
            ModelState.AddModelError(nameof(stock.LocationId), "location must be specified.");
        }
        if (!string.IsNullOrWhiteSpace(stock.ProductRef) && await _context.Stocks.AnyAsync(s => s.Id != stock.Id && s.ProductRef == stock.ProductRef)) {
            ModelState.AddModelError(nameof(stock.ProductRef), "Stock With This Product Reference Already Exists.");
        }
        await FillLocation(stock);
        await ValidateStockChange(stock.Id, stock.Product, stock.ProductRef, stock.Quantity);

        if (ModelState.IsValid) {
            var existing = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == stock.Id);
            if (existing == null) {
                return NotFound();
            }

            existing.Product = stock.Product;
            existing.ProductRef = stock.ProductRef;
            existing.LocationId = stock.LocationId;
            existing.Location = stock.Location;
            existing.Quantity = Math.Max(stock.Quantity, 0);

            if (existing.LastReceiptDate == default) {
                existing.LastReceiptDate = DateTime.Now;
            }

            try {
                DsmControllerUtilities.StampUpdate(existing);
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
        await LoadLookups();
        return View(stock);
    }

    // GET: STOCKS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var stock = await _context.Stocks.FirstOrDefaultAsync(m => m.Id == id);
        if (stock == null) {
            return NotFound();
        }
        ApplyStockQuantities(new List<Stock> { stock });
        return View(stock);
    }

    // POST: STOCKS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var stock = await _context.Stocks.FindAsync(id);
        if (stock != null) {
            if (!await CanRemoveStock(stock)) {
                ModelState.AddModelError(string.Empty, "Reserved Stock Cannot Be Reduced.");
                ApplyStockQuantities(new List<Stock> { stock });
                return View("Delete", stock);
            }
            _context.Stocks.Remove(stock);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookups() {
        ViewBag.Locations = new SelectList(await _context.Locations.OrderBy(l => l.Name).ToListAsync(), "Id", "Name");
    }

    private async Task FillLocation(Stock stock) {
        if (stock.LocationId == null) {
            stock.Location = null;
            return;
        }
        var location = await _context.Locations.FindAsync(stock.LocationId.Value);
        if (location == null) {
            ModelState.AddModelError(nameof(stock.LocationId), "Location With ID " + stock.LocationId + " Was Not Found.");
            return;
        }
        stock.Location = location.Name;
    }

    private async Task ValidateStockChange(int stockId, string product, string? productRef, int quantity) {
        var stocks = await _context.Stocks.AsNoTracking().ToListAsync();
        var existing = stocks.FirstOrDefault(s => s.Id == stockId);
        string currentProduct = ProductStockKey(existing?.Product, existing?.ProductRef);
        string nextProduct = ProductStockKey(product, productRef);
        var totals = StockTotals(stocks);
        if (existing != null) {
            totals[currentProduct] = totals.GetValueOrDefault(currentProduct) - existing.Quantity;
        }
        totals[nextProduct] = totals.GetValueOrDefault(nextProduct) + quantity;
        await ValidateReservedTotals(totals, currentProduct, nextProduct);
    }

    private async Task<bool> CanRemoveStock(Stock stock) {
        var stocks = await _context.Stocks.AsNoTracking().ToListAsync();
        var totals = StockTotals(stocks);
        string product = ProductStockKey(stock.Product, stock.ProductRef);
        totals[product] = totals.GetValueOrDefault(product) - stock.Quantity;
        return await ReservedTotalsAreValid(totals, product);
    }

    private void ApplyStockQuantities(List<Stock> stocks) {
        var allStocks = _context.Stocks.AsNoTracking().ToList();
        var totals = StockTotals(allStocks);
        var reserved = ReservedProducts();
        foreach (var stock in stocks) {
            string product = ProductStockKey(stock.Product, stock.ProductRef);
            stock.ReservedQuantity = reserved.GetValueOrDefault(product);
            stock.AvailableQuantity = Math.Max(totals.GetValueOrDefault(product) - stock.ReservedQuantity, 0);
        }
    }

    private async Task ValidateReservedTotals(Dictionary<string, int> totals, params string[] products) {
        if (!await ReservedTotalsAreValid(totals, products)) {
            ModelState.AddModelError(string.Empty, "Reserved Stock Cannot Be Reduced.");
        }
    }

    private async Task<bool> ReservedTotalsAreValid(Dictionary<string, int> totals, params string[] products) {
        var reserved = await ReservedProductsAsync();
        foreach (string product in products.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct()) {
            if (totals.GetValueOrDefault(product) < reserved.GetValueOrDefault(product)) {
                return false;
            }
        }
        return true;
    }

    private Dictionary<string, int> StockTotals(List<Stock> stocks) {
        return stocks
            .Where(s => !string.IsNullOrWhiteSpace(s.Product))
            .GroupBy(s => ProductStockKey(s.Product, s.ProductRef))
            .ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity));
    }

    private Dictionary<string, int> ReservedProducts() {
        return _context.Orders
            .Include(o => o.Products)
            .Where(ReservesStock)
            .SelectMany(o => o.Products)
            .Where(p => !string.IsNullOrWhiteSpace(p.ProductName))
            .GroupBy(p => ProductStockKey(p.ProductName, p.ProductRef))
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Quantity));
    }

    private async Task<Dictionary<string, int>> ReservedProductsAsync() {
        var reservedProducts = await _context.Orders
            .Include(o => o.Products)
            .Where(o => o.Status == OrderStatus.pendingApproval || o.Status == OrderStatus.validated || o.Status == OrderStatus.ongoing)
            .SelectMany(o => o.Products)
            .Where(p => !string.IsNullOrWhiteSpace(p.ProductName))
            .Select(p => new { p.ProductName, p.ProductRef, p.Quantity })
            .ToListAsync();

        return reservedProducts
            .GroupBy(p => ProductStockKey(p.ProductName, p.ProductRef))
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Quantity));
    }

    private string ProductStockKey(string? productName, string? productRef) {
        return DsmControllerUtilities.ProductKey(productName) + "|" + DsmControllerUtilities.Clean(productRef).ToLower();
    }

    private bool ReservesStock(Order order) {
        return order.Status == OrderStatus.pendingApproval || order.Status == OrderStatus.validated || order.Status == OrderStatus.ongoing;
    }

    private bool StockExists(int? id) {
        return _context.Stocks.Any(e => e.Id == id);
    }
}
