using System.Globalization;
using System.Security.Claims;
using DSM.Data;
using DSM.Models;
using DSM.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DSM.Controllers {
    [Authorize]
    public class ProfileController : Controller {
        private readonly ApplicationDatabaseContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public ProfileController(ApplicationDatabaseContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index() {
            User? user = await CurrentUser();
            if (user == null) {
                return RedirectToAction("SignIn", "Auth");
            }
            return View(ToViewModel(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model) {
            User? user = await CurrentUser();
            if (user == null) {
                return RedirectToAction("SignIn", "Auth");
            }
            if (!ModelState.IsValid) {
                return View(model);
            }

            string username = model.Username.Trim();
            string email = model.Email.Trim().ToLowerInvariant();
            string name = model.Name.Trim();

            if (!username.Equals(user.Username, StringComparison.OrdinalIgnoreCase) && await _context.Users.AnyAsync(u => u.Username == username)) {
                ModelState.AddModelError(nameof(model.Username), "This Username Is Already In Use.");
                return View(model);
            }
            if (!email.Equals(user.Email, StringComparison.OrdinalIgnoreCase) && await _context.Users.AnyAsync(u => u.Email == email)) {
                ModelState.AddModelError(nameof(model.Email), "This Email Is Already In Use.");
                return View(model);
            }

            user.Username = username;
            user.Email = email;
            user.Name = name;
            if (!string.IsNullOrWhiteSpace(model.Password)) {
                user.Password = _passwordHasher.HashPassword(user, model.Password);
            }
            await _context.SaveChangesAsync();
            await RefreshClaims(user);
            TempData["ProfileMessage"] = "Profile updated.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<User?> CurrentUser() {
            string username = User.Identity?.Name ?? string.Empty;
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        private ProfileViewModel ToViewModel(User user) {
            return new ProfileViewModel {
                Id = (user?.Id).GetValueOrDefault().ToString(),
                Username = user.Username ?? string.Empty,
                Name = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            };
        }

        private async Task RefreshClaims(User user) {
            var claims = new List<Claim> {
                new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Name, user.Username ?? string.Empty),
                new("DisplayName", user.Name ?? user.Username ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("IsActive", user.IsActive.ToString())
            };
            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }
    }
}
