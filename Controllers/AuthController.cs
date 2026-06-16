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
    public class AuthController : Controller {
        private readonly ApplicationDatabaseContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthController(ApplicationDatabaseContext context) {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult SignIn(string? returnUrl = null) {
            ViewBag.ReturnUrl = returnUrl;
            return View(new SignInViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null) {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) {
                return View(model);
            }

            string username = model.Username.Trim();
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !VerifyPassword(user, model.Password)) {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            if (!user.IsActive) {
                ModelState.AddModelError(string.Empty, "Your account is waiting for administrator activation.");
                return View(model);
            }

            await SignInUser(user, model.Remember);
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Dashboard");
        }

        [AllowAnonymous]
        public IActionResult SignUp() {
            return View(new SignUpViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            string username = model.Username.Trim();
            string email = model.Email.Trim().ToLowerInvariant();
            string name = model.Name.Trim();

            if (await _context.Users.AnyAsync(u => u.Username == username)) {
                ModelState.AddModelError(nameof(model.Username), "This Username Is Already In Use.");
                return View(model);
            }
            if (await _context.Users.AnyAsync(u => u.Email == email)) {
                ModelState.AddModelError(nameof(model.Email), "This Email Is Already In Use.");
                return View(model);
            }

            User user = new() {
                Username = username,
                Name = name,
                Email = email,
                Role = Role.user,
                IsActive = false,
                CreatedAt = DateTime.Now
            };
            user.Password = _passwordHasher.HashPassword(user, model.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await CreateAccountActivationTicket(user);
            TempData["AuthMessage"] = "Account request sent. Ask your administrator to activate your account.";
            return RedirectToAction(nameof(SignIn));
        }

        [Authorize]
        public async Task<IActionResult> Me() {
            User? user = await CurrentUser();
            if (user == null) {
                return RedirectToAction(nameof(SignIn));
            }
            await SignInUser(user, true);
            return RedirectToAction("Index", "Profile");
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword() {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model) {
            if (!ModelState.IsValid) {
                return View(model);
            }

            string email = model.Email.Trim().ToLowerInvariant();
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) {
                model.Error = "No User Found With This Email.";
                return View(model);
            }

            string temporaryPassword = "temp" + Random.Shared.Next(100000, 999999).ToString(CultureInfo.InvariantCulture);
            user.Password = _passwordHasher.HashPassword(user, temporaryPassword);
            await _context.SaveChangesAsync();
            model.Message = "Temporary password: " + temporaryPassword;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSignInTicket(string? TicketEmail, string? TicketCategory, string? TicketDescription) {
            if (string.IsNullOrWhiteSpace(TicketDescription) || TicketDescription.Trim().Length < 10) {
                TempData["TicketMessage"] = "Write at least 10 characters.";
                return RedirectToAction(nameof(SignIn));
            }

            SupportTicket ticket = new() {
                Subject = "Sign In Access",
                Description = TicketDescription.Trim(),
                Category = ParseEnum(TicketCategory, Category.access),
                Priority = Priority.high,
                Status = Status.open,
                RequesterEmail = TicketEmail?.Trim().ToLowerInvariant(),
                RequesterName = "Access request",
                CreatedAt = DateTime.Now
            };
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();
            TempData["TicketMessage"] = "Ticket sent.";
            return RedirectToAction(nameof(SignIn));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePublicTicket(ForgotPasswordViewModel model) {
            if (string.IsNullOrWhiteSpace(model.TicketDescription) || model.TicketDescription.Trim().Length < 10) {
                model.TicketMessage = "Write at least 10 characters.";
                return View(nameof(ForgotPassword), model);
            }

            SupportTicket ticket = new() {
                Subject = "Password Access",
                Description = model.TicketDescription.Trim(),
                Category = ParseEnum(model.TicketCategory, Category.access),
                Priority = Priority.high,
                Status = Status.open,
                RequesterEmail = model.TicketEmail?.Trim().ToLowerInvariant(),
                RequesterName = "Access request",
                CreatedAt = DateTime.Now
            };
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();
            model.TicketMessage = "Ticket sent.";
            return View(nameof(ForgotPassword), model);
        }

        [Authorize]
        public async Task<IActionResult> SignOutUser() {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(SignIn));
        }

        private async Task<User?> CurrentUser() {
            string username = User.Identity?.Name ?? string.Empty;
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        private bool VerifyPassword(User user, string password) {
            if (string.IsNullOrWhiteSpace(user.Password)) {
                return false;
            }
            PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (result != PasswordVerificationResult.Failed) {
                return true;
            }
            return user.Password == password;
        }

        private async Task SignInUser(User user, bool remember) {
            var claims = new List<Claim> {
                new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Name, user.Username ?? string.Empty),
                new("DisplayName", user.Name ?? user.Username ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("IsActive", user.IsActive.ToString())
            };
            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties {
                IsPersistent = remember,
                ExpiresUtc = remember ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
            });
        }

        private async Task CreateAccountActivationTicket(User user) {
            SupportTicket ticket = new() {
                Subject = "Account Activation",
                Description = "Activate user account: " + user.Username,
                Category = Category.accountActivation,
                Priority = Priority.high,
                Status = Status.open,
                RequesterId = user.Id,
                RequesterName = user.Name,
                RequesterEmail = user.Email,
                CreatedAt = DateTime.Now
            };
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();
        }

        private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback) where TEnum : struct {
            return Enum.TryParse(value, true, out TEnum parsed) ? parsed : fallback;
        }
    }
}
