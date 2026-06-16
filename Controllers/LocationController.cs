
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

public class LocationController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public LocationController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: LOCATIONS
    public async Task<IActionResult> Index() {
        return View(await _context.Locations.ToListAsync());
    }

    // GET: LOCATIONS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var location = await _context.Locations
            .FirstOrDefaultAsync(m => m.Id == id);
        if (location == null) {
            return NotFound();
        }

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
        if (ModelState.IsValid) {
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

        if (ModelState.IsValid) {
            try {
                _context.Update(location);
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

        var location = await _context.Locations
            .FirstOrDefaultAsync(m => m.Id == id);
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
            _context.Locations.Remove(location);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool LocationExists(int? id) {
        return _context.Locations.Any(e => e.Id == id);
    }
}
