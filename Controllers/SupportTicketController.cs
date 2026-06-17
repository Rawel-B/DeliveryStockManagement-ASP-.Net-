using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

[Authorize]
public class SupportTicketController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public SupportTicketController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: SUPPORTTICKETS
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> Index(string? criteria) {
        var tickets = _context.SupportTickets.AsQueryable();
        if (!string.IsNullOrWhiteSpace(criteria)) {
            string value = criteria.Trim();
            tickets = tickets.Where(t => (t.Subject != null && t.Subject.Contains(value)) || (t.RequesterName != null && t.RequesterName.Contains(value)) || (t.RequesterEmail != null && t.RequesterEmail.Contains(value)));
        }
        ViewBag.Criteria = criteria;
        return View(await tickets.OrderByDescending(t => t.CreatedAt).ToListAsync());
    }

    // GET: SUPPORTTICKETS/Details/5
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var supportticket = await _context.SupportTickets.FirstOrDefaultAsync(m => m.Id == id);
        if (supportticket == null) {
            return NotFound();
        }
        return View(supportticket);
    }

    // GET: SUPPORTTICKETS/Create
    public async Task<IActionResult> Create() {
        await LoadLookups();
        return View();
    }

    // POST: SUPPORTTICKETS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Subject,Description,Category,Priority,Status,RequesterId,RequesterName,RequesterEmail,AssignedUserId,AssignedUserName,CreatedAt,UpdatedAt")] SupportTicket supportticket) {
        var requester = await CurrentUser();
        if (requester == null) {
            return RedirectToAction("SignIn", "Auth");
        }
        await AssignTicket(supportticket);
        if (ModelState.IsValid) {
            supportticket.Subject = DsmControllerUtilities.CleanNullable(supportticket.Subject);
            supportticket.Description = DsmControllerUtilities.CleanNullable(supportticket.Description);
            supportticket.Status = Status.open;
            supportticket.RequesterId = requester.Id;
            supportticket.RequesterName = requester.Name;
            supportticket.RequesterEmail = requester.Email;
            DsmControllerUtilities.StampNew(supportticket);
            _context.Add(supportticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await LoadLookups();
        return View(supportticket);
    }

    // GET: SUPPORTTICKETS/Edit/5
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var supportticket = await _context.SupportTickets.FindAsync(id);
        if (supportticket == null) {
            return NotFound();
        }
        await LoadLookups();
        return View(supportticket);
    }

    // POST: SUPPORTTICKETS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Subject,Description,Category,Priority,Status,RequesterId,RequesterName,RequesterEmail,AssignedUserId,AssignedUserName,CreatedAt,UpdatedAt")] SupportTicket supportticket) {
        supportticket.Subject = DsmControllerUtilities.Clean(supportticket.Subject);
        supportticket.Description = DsmControllerUtilities.Clean(supportticket.Description);
        if (string.IsNullOrWhiteSpace(supportticket.Subject)) {
            ModelState.AddModelError(nameof(supportticket.Subject), "subject must be filled.");
        }
        if (string.IsNullOrWhiteSpace(supportticket.Description)) {
            ModelState.AddModelError(nameof(supportticket.Description), "description must be filled.");
        }
        if (id != supportticket.Id) {
            return NotFound();
        }
        await AssignTicket(supportticket);
        if (ModelState.IsValid) {
            try {
                DsmControllerUtilities.StampUpdate(supportticket);
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
        await LoadLookups();
        return View(supportticket);
    }

    // GET: SUPPORTTICKETS/Delete/5
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var supportticket = await _context.SupportTickets.FirstOrDefaultAsync(m => m.Id == id);
        if (supportticket == null) {
            return NotFound();
        }
        return View(supportticket);
    }

    // POST: SUPPORTTICKETS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var supportticket = await _context.SupportTickets.FindAsync(id);
        if (supportticket != null) {
            _context.SupportTickets.Remove(supportticket);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> UpdateStatus(int id, Status status, int? assignedUserId) {
        var ticket = await _context.SupportTickets.FindAsync(id);
        if (ticket == null) {
            return NotFound();
        }
        ticket.Status = status;
        ticket.AssignedUserId = assignedUserId;
        await AssignTicket(ticket);
        DsmControllerUtilities.StampUpdate(ticket);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadLookups() {
        ViewBag.Users = new SelectList(await _context.Users.Where(u => u.IsActive).OrderBy(u => u.Name).ToListAsync(), "Id", "Name");
    }

    private async Task AssignTicket(SupportTicket ticket) {
        if (ticket.AssignedUserId == null) {
            ticket.AssignedUserName = null;
            return;
        }
        var assignedUser = await _context.Users.FindAsync(ticket.AssignedUserId.Value);
        if (assignedUser == null) {
            ModelState.AddModelError(nameof(ticket.AssignedUserId), "Assigned User Was Not Found.");
            return;
        }
        ticket.AssignedUserName = assignedUser.Name;
    }

    private async Task<User?> CurrentUser() {
        string username = DsmControllerUtilities.CurrentUsername(this);
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    private bool SupportTicketExists(int? id) {
        return _context.SupportTickets.Any(e => e.Id == id);
    }
}
