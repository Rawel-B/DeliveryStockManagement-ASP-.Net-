using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

[Authorize(Roles = "administrator,manager,user")]
public class ShippingController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public ShippingController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: SHIPPINGS
    public async Task<IActionResult> Index(ShippingStatus? status) {
        var shippings = _context.Shippings.Include(s => s.Order).Include(s => s.Carrier).AsQueryable();
        if (status != null) {
            shippings = shippings.Where(s => s.Status == status);
        }
        var list = await shippings.OrderByDescending(s => s.CreatedAt).ToListAsync();
        foreach (var shipping in list) {
            shipping.OrderNumber = shipping.Order?.OrderNumber;
        }
        ViewBag.Status = status;
        return View(list);
    }

    // GET: SHIPPINGS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var shipping = await _context.Shippings.Include(s => s.Order).Include(s => s.Carrier).FirstOrDefaultAsync(m => m.Id == id);
        if (shipping == null) {
            return NotFound();
        }
        shipping.OrderNumber = shipping.Order?.OrderNumber;
        return View(shipping);
    }

    // GET: SHIPPINGS/Create
    public async Task<IActionResult> Create() {
        await LoadLookups();
        return View(new Shipping { DeliveryDate = DateTime.Now });
    }

    // POST: SHIPPINGS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,OrderId,CarrierId,DeliveryDate,ReceiptDate,Cost,Status,ShippingAddress,TrackingNumber,Remark,CreatedAt,UpdatedAt")] Shipping shipping) {
        shipping.ShippingAddress = DsmControllerUtilities.Clean(shipping.ShippingAddress);
        if (shipping.CarrierId == null) {
            ModelState.AddModelError(nameof(shipping.CarrierId), "carrier must be specified.");
        }
        if (shipping.DeliveryDate == null || shipping.DeliveryDate == default) {
            ModelState.AddModelError(nameof(shipping.DeliveryDate), "delivery date must be specified.");
        }
        if (string.IsNullOrWhiteSpace(shipping.ShippingAddress)) {
            ModelState.AddModelError(nameof(shipping.ShippingAddress), "shipping address must be filled.");
        }
        var order = await _context.Orders.FindAsync(shipping.OrderId);
        if (order == null) {
            ModelState.AddModelError(nameof(shipping.OrderId), "Order Was Not Found.");
        } else if (order.Status != OrderStatus.validated && order.Status != OrderStatus.ongoing) {
            ModelState.AddModelError(nameof(shipping.OrderId), "Only Validated Or Ongoing Orders Can Have Deliveries.");
        }
        if (shipping.CarrierId != null) {
            var carrier = await _context.Carriers.FindAsync(shipping.CarrierId.Value);
            if (carrier == null) {
                ModelState.AddModelError(nameof(shipping.CarrierId), "Carrier Was Not Found.");
            } else if (!carrier.IsActive) {
                ModelState.AddModelError(nameof(shipping.CarrierId), "Inactive Carriers Cannot Be Assigned To Deliveries.");
            }
        }

        if (ModelState.IsValid) {
            shipping.Status = ShippingStatus.inPerparation;
            shipping.ShippingAddress = DsmControllerUtilities.Clean(shipping.ShippingAddress);
            shipping.TrackingNumber = DsmControllerUtilities.CleanNullable(shipping.TrackingNumber);
            shipping.Remark = DsmControllerUtilities.CleanNullable(shipping.Remark);
            DsmControllerUtilities.StampNew(shipping);
            _context.Add(shipping);
            if (order != null) {
                order.Status = OrderStatus.ongoing;
                DsmControllerUtilities.StampUpdate(order);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await LoadLookups();
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
        await LoadLookups();
        return View(shipping);
    }

    // POST: SHIPPINGS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,OrderId,CarrierId,DeliveryDate,ReceiptDate,Cost,Status,ShippingAddress,TrackingNumber,Remark,CreatedAt,UpdatedAt")] Shipping shipping) {
        shipping.ShippingAddress = DsmControllerUtilities.Clean(shipping.ShippingAddress);
        if (shipping.CarrierId == null) {
            ModelState.AddModelError(nameof(shipping.CarrierId), "carrier must be specified.");
        }
        if (shipping.DeliveryDate == null || shipping.DeliveryDate == default) {
            ModelState.AddModelError(nameof(shipping.DeliveryDate), "delivery date must be specified.");
        }
        if (string.IsNullOrWhiteSpace(shipping.ShippingAddress)) {
            ModelState.AddModelError(nameof(shipping.ShippingAddress), "shipping address must be filled.");
        }
        if (id != shipping.Id) {
            return NotFound();
        }

        var existing = await _context.Shippings.FindAsync(shipping.Id);
        if (existing == null) {
            return NotFound();
        }
        if (existing.Status != ShippingStatus.inPerparation) {
            ModelState.AddModelError(string.Empty, "Only Deliveries In Preparation Can Be Modified.");
        }
        if (shipping.CarrierId != null) {
            var carrier = await _context.Carriers.FindAsync(shipping.CarrierId.Value);
            if (carrier == null) {
                ModelState.AddModelError(nameof(shipping.CarrierId), "Carrier Was Not Found.");
            } else if (!carrier.IsActive) {
                ModelState.AddModelError(nameof(shipping.CarrierId), "Inactive Carriers Cannot Be Assigned To Deliveries.");
            }
        }

        if (ModelState.IsValid) {
            existing.CarrierId = shipping.CarrierId;
            existing.DeliveryDate = shipping.DeliveryDate;
            existing.Cost = shipping.Cost;
            existing.ShippingAddress = DsmControllerUtilities.CleanNullable(shipping.ShippingAddress);
            existing.TrackingNumber = DsmControllerUtilities.CleanNullable(shipping.TrackingNumber);
            existing.Remark = DsmControllerUtilities.CleanNullable(shipping.Remark);
            DsmControllerUtilities.StampUpdate(existing);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await LoadLookups();
        return View(shipping);
    }

    // GET: SHIPPINGS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var shipping = await _context.Shippings.Include(s => s.Order).Include(s => s.Carrier).FirstOrDefaultAsync(m => m.Id == id);
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
            if (shipping.Status != ShippingStatus.inPerparation && shipping.Status != ShippingStatus.failed && shipping.Status != ShippingStatus.returned) {
                ModelState.AddModelError(string.Empty, "Only Deliveries In Preparation, Failed, Or Returned Can Be Removed.");
                return View("Delete", shipping);
            }
            _context.Shippings.Remove(shipping);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetShippingStatus(int id, ShippingStatus status) {
        var shipping = await _context.Shippings.FindAsync(id);
        if (shipping == null) {
            return NotFound();
        }
        string? error = ValidateStatusChange(shipping.Status, status);
        if (error != null) {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Details), new { id });
        }
        bool shouldDeductStock = shipping.Status != ShippingStatus.delivered && status == ShippingStatus.delivered;
        shipping.Status = status;
        DsmControllerUtilities.StampUpdate(shipping);
        if (status == ShippingStatus.delivered) {
            var order = await _context.Orders.Include(o => o.Products).FirstOrDefaultAsync(o => o.Id == shipping.OrderId);
            if (order != null) {
                if (shouldDeductStock) {
                    await DeductOrder(order);
                }
                shipping.ReceiptDate = DateTime.Now;
                order.Status = OrderStatus.delivered;
                DsmControllerUtilities.StampUpdate(order);
            }
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadLookups() {
        ViewBag.Orders = new SelectList(await _context.Orders.Where(o => o.Status == OrderStatus.validated || o.Status == OrderStatus.ongoing).OrderByDescending(o => o.OrderDate).ToListAsync(), "Id", "OrderNumber");
        ViewBag.Carriers = new SelectList(await _context.Carriers.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
    }

    private string? ValidateStatusChange(ShippingStatus current, ShippingStatus next) {
        if (current == next) return null;
        bool allowed = current switch {
            ShippingStatus.inPerparation => next == ShippingStatus.shipped || next == ShippingStatus.failed,
            ShippingStatus.shipped => next == ShippingStatus.inTransit || next == ShippingStatus.failed || next == ShippingStatus.returned,
            ShippingStatus.inTransit => next == ShippingStatus.delivered || next == ShippingStatus.failed || next == ShippingStatus.returned,
            ShippingStatus.failed => next == ShippingStatus.inPerparation || next == ShippingStatus.returned,
            ShippingStatus.delivered or ShippingStatus.returned => false,
            _ => false
        };
        return allowed ? null : "This Delivery Cannot Move To That Status.";
    }

    private async Task DeductOrder(Order order) {
        var requested = order.Products.Where(p => !string.IsNullOrWhiteSpace(p.ProductName)).GroupBy(p => DsmControllerUtilities.ProductKey(p.ProductName)).ToDictionary(g => g.Key, g => g.Sum(p => p.Quantity));
        foreach (var item in requested) {
            await DeductProductFromStock(item.Key, item.Value);
        }
        var emptyStocks = await _context.Stocks.Where(s => s.Quantity <= 0).ToListAsync();
        _context.Stocks.RemoveRange(emptyStocks);
    }

    private async Task DeductProductFromStock(string product, int quantity) {
        int remaining = quantity;
        var stocks = await _context.Stocks.Where(s => s.Product.ToLower() == product).OrderBy(s => s.CreatedAt).ToListAsync();
        int available = stocks.Sum(s => s.Quantity);
        if (available < quantity) {
            throw new InvalidOperationException("Requested Quantity Exceeds Available Stock.");
        }
        foreach (var stock in stocks) {
            if (remaining <= 0) break;
            int deductedQuantity = Math.Min(stock.Quantity, remaining);
            stock.Quantity -= deductedQuantity;
            DsmControllerUtilities.StampUpdate(stock);
            remaining -= deductedQuantity;
        }
    }

    private bool ShippingExists(int? id) {
        return _context.Shippings.Any(e => e.Id == id);
    }
}
