using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DSM.Models;
using DSM.Data;

[Authorize(Roles = "administrator")]
public class UserController : Controller {
    private readonly ApplicationDatabaseContext _context;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public UserController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: USERS
    public async Task<IActionResult> Index() {
        return View(await _context.Users.OrderBy(u => u.Username).ToListAsync());
    }

    // GET: USERS/Details/5
    public async Task<IActionResult> Details(int? id) {
        if (id == null) {
            return NotFound();
        }

        var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
        if (user == null) {
            return NotFound();
        }
        return View(user);
    }

    // GET: USERS/Create
    public IActionResult Create() {
        return View();
    }

    // POST: USERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Username,Password,Name,Email,Role,IsActive,CreatedAt")] User user) {
        user.Username = DsmControllerUtilities.Clean(user.Username);
        user.Email = DsmControllerUtilities.Clean(user.Email).ToLowerInvariant();
        user.Name = DsmControllerUtilities.Clean(user.Name);
        if (await _context.Users.AnyAsync(u => u.Username == user.Username)) {
            ModelState.AddModelError(nameof(user.Username), "This Username Is Already In Use.");
        }
        if (await _context.Users.AnyAsync(u => u.Email == user.Email)) {
            ModelState.AddModelError(nameof(user.Email), "This Email Is Already In Use.");
        }
        if (string.IsNullOrWhiteSpace(user.Password)) {
            ModelState.AddModelError(nameof(user.Password), "Password must be filled.");
        }
        if (ModelState.IsValid) {
            user.Password = _passwordHasher.HashPassword(user, user.Password!);
            user.CreatedAt = DateTime.Now;
            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // GET: USERS/Edit/5
    public async Task<IActionResult> Edit(int? id) {
        if (id == null) {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null) {
            return NotFound();
        }
        user.Password = null;
        return View(user);
    }

    // POST: USERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,Username,Password,Name,Email,Role,IsActive,CreatedAt")] User user) {
        if (id != user.Id) {
            return NotFound();
        }

        var existing = await _context.Users.FindAsync(user.Id);
        if (existing == null) {
            return NotFound();
        }
        string username = DsmControllerUtilities.Clean(user.Username);
        string email = DsmControllerUtilities.Clean(user.Email).ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Id != user.Id && u.Username == username)) {
            ModelState.AddModelError(nameof(user.Username), "This Username Is Already In Use.");
        }
        if (await _context.Users.AnyAsync(u => u.Id != user.Id && u.Email == email)) {
            ModelState.AddModelError(nameof(user.Email), "This Email Is Already In Use.");
        }
        if (existing.Username == DsmControllerUtilities.CurrentUsername(this) && !user.IsActive) {
            ModelState.AddModelError(nameof(user.IsActive), "You cannot lock your own account.");
        }

        if (ModelState.IsValid) {
            existing.Username = username;
            existing.Name = DsmControllerUtilities.Clean(user.Name);
            existing.Email = email;
            existing.Role = user.Role;
            existing.IsActive = user.IsActive;
            if (!string.IsNullOrWhiteSpace(user.Password)) {
                existing.Password = _passwordHasher.HashPassword(existing, user.Password);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // GET: USERS/Delete/5
    public async Task<IActionResult> Delete(int? id) {
        if (id == null) {
            return NotFound();
        }

        var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
        if (user == null) {
            return NotFound();
        }
        return View(user);
    }

    // POST: USERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id) {
        var user = await _context.Users.FindAsync(id);
        if (user != null) {
            if (user.Username == DsmControllerUtilities.CurrentUsername(this)) {
                ModelState.AddModelError(string.Empty, "You cannot delete your own account.");
                return View("Delete", user);
            }
            _context.Users.Remove(user);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool UserExists(int? id) {
        return _context.Users.Any(e => e.Id == id);
    }
}
