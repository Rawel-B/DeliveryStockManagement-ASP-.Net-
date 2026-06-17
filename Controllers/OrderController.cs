using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

[Authorize(Roles = "administrator,manager")]
public class OrderController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public OrderController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: ORDERS
    public async Task<IActionResult> Index(OrderStatus? status, int? customerId, int? supplierId) {
        var orders = _context.Orders.Include(o => o.Customer).Include(o => o.Supplier).Include(o => o.Products).AsQueryable();
        if (status != null) {
            orders = orders.Where(o => o.Status == status);
        }
        if (customerId != null) {
            orders = orders.Where(o => o.CustomerId == customerId);
        }
        if (supplierId != null) {
            orders = orders.Where(o => o.SupplierId == supplierId);
        }
        var list = await orders.OrderByDescending(o => o.OrderDate).ToListAsync();
        foreach (var order in list) {
            order.CustomerName = order.Customer?.Name;
            order.SupplierName = order.Supplier?.Name;
        }
        ViewBag.Status = status;
        return View(list);
    }

    // GET: ORDERS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var order = await _context.Orders.Include(o => o.Customer).Include(o => o.Supplier).Include(o => o.Products).Include(o => o.Shippings).Include(o => o.Invoices).FirstOrDefaultAsync(m => m.Id == id);
        if (order == null) {
            return NotFound();
        }
        order.CustomerName = order.Customer?.Name;
        order.SupplierName = order.Supplier?.Name;
        return View(order);
    }

    // GET: ORDERS/Create
    public async Task<IActionResult> Create() {
        await LoadLookups();
        return View(new Order { OrderDate = DateTime.Now, Status = OrderStatus.pendingApproval, Products = new List<Product>() });
    }

    // POST: ORDERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,CustomerId,SupplierId,OrderDate,Status,TotalAmount,OrderNumber,Remark,Products,CreatedAt,UpdatedAt")] Order order) {
        await ValidateOrderRequest(order, null);
        if (ModelState.IsValid) {
            order.Status = OrderStatus.pendingApproval;
            order.Init();
            order.TotalAmount = 0m;
            order.Products = CleanProducts(order.Products);
            foreach (var product in order.Products) {
                product.CalculateSubTotal();
                DsmControllerUtilities.StampNew(product);
                order.TotalAmount += product.SubTotal;
            }
            DsmControllerUtilities.StampNew(order);
            _context.Add(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        AddOrderErrorToast();
        await LoadLookups();
        return View(order);
    }

    // GET: ORDERS/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var order = await _context.Orders.Include(o => o.Products).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) {
            return NotFound();
        }
        await LoadLookups();
        return View(order);
    }

    // POST: ORDERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,CustomerId,SupplierId,OrderDate,Status,TotalAmount,OrderNumber,Remark,Products,CreatedAt,UpdatedAt")] Order order) {
        if (id != order.Id) {
            return NotFound();
        }

        var existing = await _context.Orders.Include(o => o.Products).FirstOrDefaultAsync(o => o.Id == order.Id);
        if (existing == null) {
            return NotFound();
        }
        if (existing.Status != OrderStatus.pendingApproval && existing.Status != OrderStatus.validated) {
            ModelState.AddModelError(string.Empty, "Only Pending Approval Or Validated Orders Can Be Modified.");
        }
        await ValidateOrderRequest(order, order.Id);

        if (ModelState.IsValid) {
            existing.CustomerId = order.CustomerId;
            existing.SupplierId = order.SupplierId;
            existing.Remark = DsmControllerUtilities.CleanNullable(order.Remark);
            existing.Products.Clear();
            existing.TotalAmount = 0m;
            foreach (var product in CleanProducts(order.Products)) {
                product.OrderId = existing.Id;
                product.CalculateSubTotal();
                DsmControllerUtilities.StampNew(product);
                existing.Products.Add(product);
                existing.TotalAmount += product.SubTotal;
            }
            DsmControllerUtilities.StampUpdate(existing);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        AddOrderErrorToast();
        await LoadLookups();
        return View(order);
    }

    // GET: ORDERS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var order = await _context.Orders.Include(o => o.Customer).Include(o => o.Supplier).Include(o => o.Products).FirstOrDefaultAsync(m => m.Id == id);
        if (order == null) {
            return NotFound();
        }
        return View(order);
    }

    // POST: ORDERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var order = await _context.Orders.Include(o => o.Products).FirstOrDefaultAsync(o => o.Id == id);
        if (order != null) {
            if (order.Status != OrderStatus.pendingApproval && order.Status != OrderStatus.cancelled) {
                ModelState.AddModelError(string.Empty, "You May Only Remove Orders That Are Pending Approval Or Cancelled.");
                return View("Delete", order);
            }
            _context.Products.RemoveRange(order.Products);
            _context.Orders.Remove(order);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidateOrder(int id) {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) {
            return NotFound();
        }
        if (order.Status != OrderStatus.pendingApproval) {
            TempData["ToastMessage"] = "Only Pending Approval Orders Can Be Validated.";
            TempData["ToastType"] = "error";
            return RedirectToAction(nameof(Index));
        }
        order.Status = OrderStatus.validated;
        DsmControllerUtilities.StampUpdate(order);
        await _context.SaveChangesAsync();
        TempData["ToastMessage"] = "Order validated.";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetOrderStatus(int id, OrderStatus status) {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) {
            return NotFound();
        }
        string? error = ValidateStatusChange(order.Status, status);
        if (error != null) {
            TempData["ToastMessage"] = error;
            TempData["ToastType"] = "error";
            return RedirectToAction(nameof(Index));
        }
        order.Status = status;
        DsmControllerUtilities.StampUpdate(order);
        await _context.SaveChangesAsync();
        TempData["ToastMessage"] = "Order moved to " + DsmControllerUtilities.DisplayLabel(status) + ".";
        TempData["ToastType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    private void AddOrderErrorToast() {
        var message = ModelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(error => !string.IsNullOrWhiteSpace(error));

        if (!string.IsNullOrWhiteSpace(message)) {
            TempData["ToastMessage"] = message;
            TempData["ToastType"] = "error";
        }
    }

    private async Task LoadLookups() {
        ViewBag.Customers = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        ViewBag.Suppliers = new SelectList(await _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(), "Id", "Name");

        var stocks = await _context.Stocks
            .AsNoTracking()
            .Where(s => !string.IsNullOrWhiteSpace(s.Product) && s.Quantity > 0)
            .ToListAsync();

        var reserved = await ReservedProductsAsync();

        ViewBag.StockProducts = stocks
            .GroupBy(s => ProductStockKey(s.Product, s.ProductRef))
            .Select(group => new {
                ProductName = group.First().Product,
                ProductRef = group.Select(s => s.ProductRef).FirstOrDefault(reference => !string.IsNullOrWhiteSpace(reference)),
                Quantity = Math.Max(group.Sum(s => s.Quantity) - reserved.GetValueOrDefault(group.Key), 0)
            })
            .Where(item => item.Quantity > 0)
            .OrderBy(item => item.ProductName)
            .ThenBy(item => item.ProductRef)
            .ToList();
    }

    private async Task ValidateOrderRequest(Order order, int? ignoredOrderId) {
        if (!await _context.Customers.AnyAsync(c => c.Id == order.CustomerId)) {
            ModelState.AddModelError(nameof(order.CustomerId), "Customer With " + order.CustomerId + " Was Not Found.");
        }

        if (order.SupplierId == null || !await _context.Suppliers.AnyAsync(s => s.Id == order.SupplierId && s.IsActive)) {
            ModelState.AddModelError(nameof(order.SupplierId), "Supplier Must Be Specified.");
        }

        var products = CleanProducts(order.Products);

        if (!products.Any()) {
            ModelState.AddModelError(nameof(order.Products), "At Least One Product Must Be Added.");
            return;
        }

        var available = await AvailableStockByProduct(ignoredOrderId);

        var requested = products
            .GroupBy(p => ProductStockKey(p.ProductName, p.ProductRef))
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Quantity));

        foreach (var item in requested) {
            if (available.GetValueOrDefault(item.Key) < item.Value) {
                ModelState.AddModelError(nameof(order.Products), "Requested Quantity Exceeds Available Stock.");
                return;
            }
        }
    }

    private List<Product> CleanProducts(List<Product>? products) {
        return (products ?? new List<Product>())
            .Where(p => !string.IsNullOrWhiteSpace(p.ProductName))
            .Select(p => {
                p.ProductName = DsmControllerUtilities.Clean(p.ProductName);
                p.ProductRef = DsmControllerUtilities.CleanNullable(p.ProductRef);
                p.Quantity = Math.Max(1, p.Quantity);
                p.CalculateSubTotal();
                return p;
            })
            .ToList();
    }

    private async Task<Dictionary<string, int>> ReservedProductsAsync(int? ignoredOrderId = null) {
        var reservedQuery = _context.Orders
            .Where(o =>
                o.Status == OrderStatus.pendingApproval ||
                o.Status == OrderStatus.validated ||
                o.Status == OrderStatus.ongoing);

        if (ignoredOrderId != null) {
            reservedQuery = reservedQuery.Where(o => o.Id != ignoredOrderId);
        }

        var reservedProducts = await reservedQuery
            .SelectMany(o => o.Products)
            .Where(p => !string.IsNullOrWhiteSpace(p.ProductName))
            .Select(p => new {
                p.ProductName,
                p.ProductRef,
                p.Quantity
            })
            .ToListAsync();

        return reservedProducts
            .GroupBy(p => ProductStockKey(p.ProductName, p.ProductRef))
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Quantity));
    }

    private async Task<Dictionary<string, int>> AvailableStockByProduct(int? ignoredOrderId) {
        var stocks = await _context.Stocks
            .AsNoTracking()
            .Where(s => !string.IsNullOrWhiteSpace(s.Product))
            .ToListAsync();

        var totals = stocks
            .GroupBy(s => ProductStockKey(s.Product, s.ProductRef))
            .ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity));

        var reserved = await ReservedProductsAsync(ignoredOrderId);

        foreach (var item in reserved) {
            totals[item.Key] = totals.GetValueOrDefault(item.Key) - item.Value;
        }

        return totals;
    }

    private string ProductStockKey(string? productName, string? productRef) {
        return DsmControllerUtilities.ProductKey(productName) + "|" + DsmControllerUtilities.Clean(productRef).ToLower();
    }

    private string? ValidateStatusChange(OrderStatus current, OrderStatus next) {
        if (current == next) return null;
        if (current == OrderStatus.pendingApproval) {
            return next == OrderStatus.validated || next == OrderStatus.cancelled ? null : "Pending Approval Orders Must Be Validated Before Moving Forward.";
        }
        if (current == OrderStatus.validated) {
            return next == OrderStatus.cancelled ? null : "Create A Delivery To Move This Order Forward.";
        }
        if (current == OrderStatus.ongoing) return "Update The Related Delivery To Complete This Order.";
        if (current == OrderStatus.delivered || current == OrderStatus.cancelled) return "This Order Is Already Closed.";
        return null;
    }

    private bool OrderExists(int? id) {
        return _context.Orders.Any(e => e.Id == id);
    }
}
