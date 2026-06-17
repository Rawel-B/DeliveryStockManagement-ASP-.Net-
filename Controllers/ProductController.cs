using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

[Authorize]
public class ProductController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public ProductController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: PRODUCTS
    public async Task<IActionResult> Index(int? orderId) {
        var products = _context.Products.AsQueryable();
        if (orderId != null) {
            products = products.Where(p => p.OrderId == orderId);
        }
        ViewBag.OrderId = orderId;
        return View(await products.OrderBy(p => p.ProductName).ToListAsync());
    }

    // GET: PRODUCTS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var product = await _context.Products.Include(p => p.Order).FirstOrDefaultAsync(m => m.Id == id);
        if (product == null) {
            return NotFound();
        }
        return View(product);
    }

    // GET: PRODUCTS/Create
    public async Task<IActionResult> Create() {
        await LoadLookups();
        return View();
    }

    // POST: PRODUCTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,OrderId,ProductName,ProductRef,Quantity,PricePerUnit,SubTotal,CreatedAt,UpdatedAt")] Product product) {
        if (ModelState.IsValid) {
            product.ProductName = DsmControllerUtilities.Clean(product.ProductName);
            product.ProductRef = DsmControllerUtilities.CleanNullable(product.ProductRef);
            product.CalculateSubTotal();
            DsmControllerUtilities.StampNew(product);
            _context.Add(product);
            await _context.SaveChangesAsync();
            await RecalculateOrderTotal(product.OrderId);
            return RedirectToAction(nameof(Index));
        }
        await LoadLookups();
        return View(product);
    }

    // GET: PRODUCTS/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null) {
            return NotFound();
        }
        await LoadLookups();
        return View(product);
    }

    // POST: PRODUCTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,OrderId,ProductName,ProductRef,Quantity,PricePerUnit,SubTotal,CreatedAt,UpdatedAt")] Product product) {
        if (id != product.Id) {
            return NotFound();
        }

        if (ModelState.IsValid) {
            try {
                product.ProductName = DsmControllerUtilities.Clean(product.ProductName);
                product.ProductRef = DsmControllerUtilities.CleanNullable(product.ProductRef);
                product.CalculateSubTotal();
                DsmControllerUtilities.StampUpdate(product);
                _context.Update(product);
                await _context.SaveChangesAsync();
                await RecalculateOrderTotal(product.OrderId);
            } catch (DbUpdateConcurrencyException) {
                if (!ProductExists(product.Id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        await LoadLookups();
        return View(product);
    }

    // GET: PRODUCTS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var product = await _context.Products.Include(p => p.Order).FirstOrDefaultAsync(m => m.Id == id);
        if (product == null) {
            return NotFound();
        }
        return View(product);
    }

    // POST: PRODUCTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var product = await _context.Products.FindAsync(id);
        int? orderId = product?.OrderId;
        if (product != null) {
            _context.Products.Remove(product);
        }

        await _context.SaveChangesAsync();
        if (orderId != null) {
            await RecalculateOrderTotal(orderId.Value);
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookups() {
        ViewBag.Orders = new SelectList(await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync(), "Id", "OrderNumber");
    }

    private async Task RecalculateOrderTotal(int orderId) {
        var order = await _context.Orders.Include(o => o.Products).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) {
            return;
        }
        order.TotalAmount = order.Products.Sum(p => p.SubTotal);
        DsmControllerUtilities.StampUpdate(order);
        await _context.SaveChangesAsync();
    }

    private bool ProductExists(int? id) {
        return _context.Products.Any(e => e.Id == id);
    }
}
