using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

[Authorize]
public class LocationController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public LocationController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: LOCATIONS
    public async Task<IActionResult> Index(string? criteria) {
        var locations = _context.Locations.AsQueryable();
        if (!string.IsNullOrWhiteSpace(criteria)) {
            string value = criteria.Trim();
            locations = locations.Where(l => l.Name.Contains(value) || (l.Code != null && l.Code.Contains(value)) || (l.Description != null && l.Description.Contains(value)));
        }
        var list = await locations.OrderBy(l => l.Name).ToListAsync();
        foreach (var location in list) {
            location.StockCount = await _context.Stocks.CountAsync(s => s.LocationId == location.Id);
        }
        ViewBag.Criteria = criteria;
        return View(list);
    }

    // GET: LOCATIONS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var location = await _context.Locations.FirstOrDefaultAsync(m => m.Id == id);
        if (location == null) {
            return NotFound();
        }
        location.StockCount = await _context.Stocks.CountAsync(s => s.LocationId == location.Id);
        return View(location);
    }

    // GET: LOCATIONS/Create
    public IActionResult Create() {
        return View();
    }

    // POST: LOCATIONS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Code,Description,CreatedAt,UpdatedAt")] Location location) {
        location.Name = DsmControllerUtilities.Clean(location.Name);
        location.Code = DsmControllerUtilities.CleanNullable(location.Code);
        location.Description = DsmControllerUtilities.CleanNullable(location.Description);
        if (await _context.Locations.AnyAsync(l => l.Name == location.Name)) {
            ModelState.AddModelError(nameof(location.Name), "Location With This Name Already Exists.");
        }
        if (!string.IsNullOrWhiteSpace(location.Code) && await _context.Locations.AnyAsync(l => l.Code == location.Code)) {
            ModelState.AddModelError(nameof(location.Code), "Location With This Code Already Exists.");
        }
        if (ModelState.IsValid) {
            DsmControllerUtilities.StampNew(location);
            _context.Add(location);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(location);
    }

    // GET: LOCATIONS/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var location = await _context.Locations.FindAsync(id);
        if (location == null) {
            return NotFound();
        }
        return View(location);
    }

    // POST: LOCATIONS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Code,Description,CreatedAt,UpdatedAt")] Location location) {
        if (id != location.Id) {
            return NotFound();
        }

        location.Name = DsmControllerUtilities.Clean(location.Name);
        location.Code = DsmControllerUtilities.CleanNullable(location.Code);
        location.Description = DsmControllerUtilities.CleanNullable(location.Description);
        if (await _context.Locations.AnyAsync(l => l.Id != location.Id && l.Name == location.Name)) {
            ModelState.AddModelError(nameof(location.Name), "Location With This Name Already Exists.");
        }
        if (!string.IsNullOrWhiteSpace(location.Code) && await _context.Locations.AnyAsync(l => l.Id != location.Id && l.Code == location.Code)) {
            ModelState.AddModelError(nameof(location.Code), "Location With This Code Already Exists.");
        }

        if (ModelState.IsValid) {
            try {
                DsmControllerUtilities.StampUpdate(location);
                _context.Update(location);
                await _context.SaveChangesAsync();
                var relatedStocks = await _context.Stocks.Where(s => s.LocationId == location.Id).ToListAsync();
                foreach (var stock in relatedStocks) {
                    stock.Location = location.Name;
                    DsmControllerUtilities.StampUpdate(stock);
                }
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!LocationExists(location.Id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(location);
    }

    // GET: LOCATIONS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var location = await _context.Locations.FirstOrDefaultAsync(m => m.Id == id);
        if (location == null) {
            return NotFound();
        }
        return View(location);
    }

    // POST: LOCATIONS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var location = await _context.Locations.FindAsync(id);
        if (location != null) {
            var relatedStocks = await _context.Stocks.Where(s => s.LocationId == location.Id).ToListAsync();
            foreach (var stock in relatedStocks) {
                stock.LocationId = null;
                stock.Location = null;
                DsmControllerUtilities.StampUpdate(stock);
            }
            _context.Locations.Remove(location);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool LocationExists(int? id) {
        return _context.Locations.Any(e => e.Id == id);
    }
}
