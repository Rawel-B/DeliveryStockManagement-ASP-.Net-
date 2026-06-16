
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

public class CarrierController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public CarrierController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: CARRIERS
    public async Task<IActionResult> Index(string? search, string? status) {
        var carriers = _context.Carriers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search)) {
            carriers = carriers.Where(c =>
                c.Name.Contains(search) ||
                (c.Phone != null && c.Phone.Contains(search)));
        }

        if (status == "active") {
            carriers = carriers.Where(c => c.IsActive == true);
        }

        if (status == "inactive") {
            carriers = carriers.Where(c => c.IsActive != true);
        }

        ViewBag.Search = search;
        ViewBag.Status = status;

        return View(await carriers.ToListAsync());
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
    public async Task<IActionResult> Create([Bind("Id,Name,Phone,Rating,IsActive,ShippingIds,CreatedAt,UpdatedAt")] Carrier carrier) {
        if (ModelState.IsValid) {
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
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Phone,Rating,IsActive,ShippingIds,CreatedAt,UpdatedAt")] Carrier carrier) {
        if (id != carrier.Id) {
            return NotFound();
        }

        if (ModelState.IsValid) {
            try {
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

        var carrier = await _context.Carriers
            .FirstOrDefaultAsync(m => m.Id == id);
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
