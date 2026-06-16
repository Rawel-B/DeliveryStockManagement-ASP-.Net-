
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

public class SupportTicketController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public SupportTicketController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: SUPPORTTICKETS
    public async Task<IActionResult> Index() {
        return View(await _context.SupportTickets.ToListAsync());
    }
    // GET: SUPPORTTICKETS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var supportticket = await _context.SupportTickets
            .FirstOrDefaultAsync(m => m.Id == id);
        if (supportticket == null) {
            return NotFound();
        }

        return View(supportticket);
    }
    // GET: SUPPORTTICKETS/Create
    public IActionResult Create() {
        return View();
    }
    // POST: SUPPORTTICKETS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Subject,Description,Category,Priority,Status,RequesterId,RequesterName,RequesterEmail,AssignedUserId,AssignedUserName,CreatedAt,UpdatedAt")] SupportTicket supportticket) {
        if (ModelState.IsValid) {
            _context.Add(supportticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(supportticket);
    }
    // GET: SUPPORTTICKETS/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var supportticket = await _context.SupportTickets.FindAsync(id);
        if (supportticket == null) {
            return NotFound();
        }
        return View(supportticket);
    }
    // POST: SUPPORTTICKETS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Subject,Description,Category,Priority,Status,RequesterId,RequesterName,RequesterEmail,AssignedUserId,AssignedUserName,CreatedAt,UpdatedAt")] SupportTicket supportticket) {
        if (id != supportticket.Id) {
            return NotFound();
        }

        if (ModelState.IsValid) {
            try {
                _context.Update(supportticket);
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!SupportTicketExists(supportticket.Id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(supportticket);
    }
    // GET: SUPPORTTICKETS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var supportticket = await _context.SupportTickets
            .FirstOrDefaultAsync(m => m.Id == id);
        if (supportticket == null) {
            return NotFound();
        }

        return View(supportticket);
    }
    // POST: SUPPORTTICKETS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var supportticket = await _context.SupportTickets.FindAsync(id);
        if (supportticket != null) {
            _context.SupportTickets.Remove(supportticket);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    private bool SupportTicketExists(int? id) {
        return _context.SupportTickets.Any(e => e.Id == id);
    }

}
