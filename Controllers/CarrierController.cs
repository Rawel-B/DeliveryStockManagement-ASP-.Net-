using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

[Authorize(Roles = "administrator,manager")]
public class CarrierController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public CarrierController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: CARRIERS
    public async Task<IActionResult> Index(string? search, string? status, bool? isActive) {
        var carriers = _context.Carriers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search)) {
            string criteria = search.Trim();
            carriers = carriers.Where(c => c.Name.Contains(criteria) || (c.Phone != null && c.Phone.Contains(criteria)));
        }

        if (isActive == true || status == "active") {
            carriers = carriers.Where(c => c.IsActive);
        }
        if (status == "inactive") {
            carriers = carriers.Where(c => !c.IsActive);
        }

        var list = await carriers.OrderBy(c => c.Name).ToListAsync();
        foreach (var carrier in list) {
            carrier.ShippingsCount = await _context.Shippings.CountAsync(s => s.CarrierId == carrier.Id);
        }

        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(list);
    }

    // GET: CARRIERS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var carrier = await _context.Carriers.FirstOrDefaultAsync(m => m.Id == id);
        if (carrier == null) {
            return NotFound();
        }

        carrier.ShippingsCount = await _context.Shippings.CountAsync(s => s.CarrierId == carrier.Id);
        return View(carrier);
    }

    // GET: CARRIERS/Create
    public IActionResult Create() {
        return View();
    }

    // POST: CARRIERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Phone,Rating,IsActive,CreatedAt,UpdatedAt")] Carrier carrier) {
        if (ModelState.IsValid) {
            carrier.Name = DsmControllerUtilities.Clean(carrier.Name);
            carrier.Phone = DsmControllerUtilities.Clean(carrier.Phone);
            DsmControllerUtilities.StampNew(carrier);
            _context.Add(carrier);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(carrier);
    }

    // GET: CARRIERS/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var carrier = await _context.Carriers.FindAsync(id);
        if (carrier == null) {
            return NotFound();
        }
        return View(carrier);
    }

    // POST: CARRIERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Phone,Rating,IsActive,CreatedAt,UpdatedAt")] Carrier carrier) {
        carrier.Name = DsmControllerUtilities.Clean(carrier.Name);
        carrier.Phone = DsmControllerUtilities.Clean(carrier.Phone);
        if (string.IsNullOrWhiteSpace(carrier.Phone)) {
            ModelState.AddModelError(nameof(carrier.Phone), "phone must be filled.");
        }
        if (id != carrier.Id) {
            return NotFound();
        }

        if (ModelState.IsValid) {
            try {
                carrier.Name = DsmControllerUtilities.Clean(carrier.Name);
                carrier.Phone = DsmControllerUtilities.Clean(carrier.Phone);
                DsmControllerUtilities.StampUpdate(carrier);
                _context.Update(carrier);
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!CarrierExists(carrier.Id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(carrier);
    }

    // GET: CARRIERS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var carrier = await _context.Carriers.FirstOrDefaultAsync(m => m.Id == id);
        if (carrier == null) {
            return NotFound();
        }

        return View(carrier);
    }

    // POST: CARRIERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var carrier = await _context.Carriers.FindAsync(id);
        if (carrier != null) {
            _context.Carriers.Remove(carrier);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CarrierExists(int? id) {
        return _context.Carriers.Any(e => e.Id == id);
    }
}
