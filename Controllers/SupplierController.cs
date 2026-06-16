using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

[Authorize]
public class SupplierController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public SupplierController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: SUPPLIERS
    public async Task<IActionResult> Index(string? criteria, bool? isActive, string? status) {
        var suppliers = _context.Suppliers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(criteria)) {
            string value = criteria.Trim();
            suppliers = suppliers.Where(s => s.Name.Contains(value) || (s.Email != null && s.Email.Contains(value)) || (s.Phone != null && s.Phone.Contains(value)));
        }
        if (isActive == true || status == "active") {
            suppliers = suppliers.Where(s => s.IsActive);
        }
        if (status == "inactive") {
            suppliers = suppliers.Where(s => !s.IsActive);
        }
        var list = await suppliers.OrderBy(s => s.Name).ToListAsync();
        foreach (var supplier in list) {
            supplier.OrdersCount = await _context.Orders.CountAsync(o => o.SupplierId == supplier.Id);
        }
        ViewBag.Criteria = criteria;
        ViewBag.Status = status;
        return View(list);
    }

    // GET: SUPPLIERS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var supplier = await _context.Suppliers.FirstOrDefaultAsync(m => m.Id == id);
        if (supplier == null) {
            return NotFound();
        }
        supplier.OrdersCount = await _context.Orders.CountAsync(o => o.SupplierId == supplier.Id);
        return View(supplier);
    }

    // GET: SUPPLIERS/Create
    public IActionResult Create() {
        return View();
    }

    // POST: SUPPLIERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Email,Phone,Address,IsActive,CreatedAt,UpdatedAt")] Supplier supplier) {
        supplier.Email = DsmControllerUtilities.CleanNullable(supplier.Email)?.ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(supplier.Email) && await _context.Suppliers.AnyAsync(s => s.Email == supplier.Email)) {
            ModelState.AddModelError(nameof(supplier.Email), "a Supplier With This Email Already Exists.");
        }
        if (ModelState.IsValid) {
            supplier.Name = DsmControllerUtilities.Clean(supplier.Name);
            supplier.Phone = DsmControllerUtilities.CleanNullable(supplier.Phone);
            supplier.Address = DsmControllerUtilities.CleanNullable(supplier.Address);
            DsmControllerUtilities.StampNew(supplier);
            _context.Add(supplier);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(supplier);
    }

    // GET: SUPPLIERS/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null) {
            return NotFound();
        }
        return View(supplier);
    }

    // POST: SUPPLIERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Email,Phone,Address,IsActive,CreatedAt,UpdatedAt")] Supplier supplier) {
        if (id != supplier.Id) {
            return NotFound();
        }

        supplier.Email = DsmControllerUtilities.CleanNullable(supplier.Email)?.ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(supplier.Email) && await _context.Suppliers.AnyAsync(s => s.Id != supplier.Id && s.Email == supplier.Email)) {
            ModelState.AddModelError(nameof(supplier.Email), "a Supplier With This Email Already Exists.");
        }

        if (ModelState.IsValid) {
            try {
                supplier.Name = DsmControllerUtilities.Clean(supplier.Name);
                supplier.Phone = DsmControllerUtilities.CleanNullable(supplier.Phone);
                supplier.Address = DsmControllerUtilities.CleanNullable(supplier.Address);
                DsmControllerUtilities.StampUpdate(supplier);
                _context.Update(supplier);
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!SupplierExists(supplier.Id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(supplier);
    }

    // GET: SUPPLIERS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var supplier = await _context.Suppliers.FirstOrDefaultAsync(m => m.Id == id);
        if (supplier == null) {
            return NotFound();
        }
        return View(supplier);
    }

    // POST: SUPPLIERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier != null) {
            _context.Suppliers.Remove(supplier);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool SupplierExists(int? id) {
        return _context.Suppliers.Any(e => e.Id == id);
    }
}
