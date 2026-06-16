using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

[Authorize]
public class CustomerController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public CustomerController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: CUSTOMERS
    public async Task<IActionResult> Index(string? criteria) {
        var customers = _context.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(criteria)) {
            string value = criteria.Trim();
            customers = customers.Where(c => c.Name.Contains(value) || c.Email.Contains(value) || (c.Phone != null && c.Phone.Contains(value)));
        }
        var list = await customers.OrderBy(c => c.Name).ToListAsync();
        foreach (var customer in list) {
            customer.OrdersCount = await _context.Orders.CountAsync(o => o.CustomerId == customer.Id);
        }
        ViewBag.Criteria = criteria;
        return View(list);
    }

    // GET: CUSTOMERS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
        if (customer == null) {
            return NotFound();
        }
        customer.OrdersCount = await _context.Orders.CountAsync(o => o.CustomerId == customer.Id);
        return View(customer);
    }

    // GET: CUSTOMERS/Create
    public IActionResult Create() {
        return View();
    }

    // POST: CUSTOMERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Email,Address,Phone,CreatedAt,UpdatedAt")] Customer customer) {
        customer.Email = DsmControllerUtilities.Clean(customer.Email).ToLowerInvariant();
        if (await _context.Customers.AnyAsync(c => c.Email == customer.Email)) {
            ModelState.AddModelError(nameof(customer.Email), "a Customer With This Email Already Exists.");
        }
        if (ModelState.IsValid) {
            customer.Name = DsmControllerUtilities.Clean(customer.Name);
            customer.Address = DsmControllerUtilities.CleanNullable(customer.Address);
            customer.Phone = DsmControllerUtilities.CleanNullable(customer.Phone);
            DsmControllerUtilities.StampNew(customer);
            _context.Add(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(customer);
    }

    // GET: CUSTOMERS/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) {
            return NotFound();
        }
        return View(customer);
    }

    // POST: CUSTOMERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Email,Address,Phone,CreatedAt,UpdatedAt")] Customer customer) {
        if (id != customer.Id) {
            return NotFound();
        }

        customer.Email = DsmControllerUtilities.Clean(customer.Email).ToLowerInvariant();
        if (await _context.Customers.AnyAsync(c => c.Id != customer.Id && c.Email == customer.Email)) {
            ModelState.AddModelError(nameof(customer.Email), "a Customer With This Email Already Exists.");
        }

        if (ModelState.IsValid) {
            try {
                customer.Name = DsmControllerUtilities.Clean(customer.Name);
                customer.Address = DsmControllerUtilities.CleanNullable(customer.Address);
                customer.Phone = DsmControllerUtilities.CleanNullable(customer.Phone);
                DsmControllerUtilities.StampUpdate(customer);
                _context.Update(customer);
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!CustomerExists(customer.Id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(customer);
    }

    // GET: CUSTOMERS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
        if (customer == null) {
            return NotFound();
        }
        return View(customer);
    }

    // POST: CUSTOMERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null) {
            _context.Customers.Remove(customer);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CustomerExists(int? id) {
        return _context.Customers.Any(e => e.Id == id);
    }
}
