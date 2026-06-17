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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignIn(string? returnUrl = null) {
            if (User.Identity?.IsAuthenticated == true) {
                return RedirectToAction("Index", "Dashboard");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(new SignInViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null) {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) {
                model.LoginFailed = false;
                return View(model);
            }

            string username = model.Username.Trim();
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !VerifyPassword(user, model.Password)) {
                model.LoginFailed = true;
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            if (!IsActive(user)) {
                model.LoginFailed = true;
                ModelState.AddModelError(string.Empty, "Your account is waiting for administrator activation.");
                return View(model);
            }

            await SignInUser(user, model.Remember);
            SetToast("Signed in successfully.", "success");
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
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
                IsActive = false,
                CreatedAt = DateTime.Now
            };
            SetValue(user, "Role", "user");
            user.Password = _passwordHasher.HashPassword(user, model.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await CreateAccountActivationTicket(user);
            TempData["AuthMessage"] = "Account request sent. Ask your administrator to activate your account.";
            return RedirectToAction(nameof(SignIn));
        }

        [HttpGet]
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
            SignInViewModel model = new() {
                LoginFailed = true,
                TicketEmail = TicketEmail,
                TicketCategory = string.IsNullOrWhiteSpace(TicketCategory) ? "access" : TicketCategory,
                TicketDescription = TicketDescription
            };

            if (string.IsNullOrWhiteSpace(TicketDescription) || TicketDescription.Trim().Length < 10) {
                model.TicketMessage = "Write at least 10 characters.";
                return View(nameof(SignIn), model);
            }

            SupportTicket ticket = new() {
                Subject = "Sign In Access",
                Description = TicketDescription.Trim(),
                RequesterEmail = TicketEmail?.Trim().ToLowerInvariant(),
                RequesterName = "Access request",
                CreatedAt = DateTime.Now
            };
            SetValue(ticket, "Category", model.TicketCategory);
            SetValue(ticket, "Priority", "high");
            SetValue(ticket, "Status", "open");
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            model.TicketMessage = "Ticket sent.";
            return View(nameof(SignIn), model);
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
                RequesterEmail = model.TicketEmail?.Trim().ToLowerInvariant(),
                RequesterName = "Access request",
                CreatedAt = DateTime.Now
            };
            SetValue(ticket, "Category", string.IsNullOrWhiteSpace(model.TicketCategory) ? "access" : model.TicketCategory);
            SetValue(ticket, "Priority", "high");
            SetValue(ticket, "Status", "open");
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();
            model.TicketMessage = "Ticket sent.";
            return View(nameof(ForgotPassword), model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut() {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["AuthMessage"] = "Signed out.";
            return RedirectToAction(nameof(SignIn));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SignOutUser() {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["AuthMessage"] = "Signed out.";
            return RedirectToAction(nameof(SignIn));
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

        private bool IsActive(User user) {
            object? value = GetValue(user, "IsActive");
            return value is bool b ? b : value == null || bool.TryParse(value.ToString(), out bool parsed) && parsed;
        }

        private async Task SignInUser(User user, bool remember) {
            var claims = new List<Claim> {
                new(ClaimTypes.NameIdentifier, GetValue(user, "Id")?.ToString() ?? string.Empty),
                new(ClaimTypes.Name, user.Username ?? string.Empty),
                new("DisplayName", user.Name ?? user.Username ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Role, GetValue(user, "Role")?.ToString() ?? "user"),
                new("IsActive", IsActive(user).ToString())
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
                RequesterName = user.Name,
                RequesterEmail = user.Email,
                CreatedAt = DateTime.Now
            };
            SetValue(ticket, "Category", "accountActivation");
            SetValue(ticket, "Priority", "high");
            SetValue(ticket, "Status", "open");
            SetValue(ticket, "RequesterId", GetValue(user, "Id"));
            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();
        }

        private void SetToast(string message, string type) {
            TempData["ToastMessage"] = message;
            TempData["ToastType"] = type;
        }

        private static object? GetValue(object target, string propertyName) {
            return target.GetType().GetProperty(propertyName)?.GetValue(target);
        }

        private static void SetValue(object target, string propertyName, object? value) {
            var property = target.GetType().GetProperty(propertyName);
            if (property == null || !property.CanWrite) {
                return;
            }
            Type targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (value == null) {
                property.SetValue(target, null);
                return;
            }
            if (targetType.IsEnum) {
                property.SetValue(target, Enum.Parse(targetType, value.ToString() ?? string.Empty, true));
                return;
            }
            if (targetType == typeof(string)) {
                property.SetValue(target, value.ToString());
                return;
            }
            if (targetType == typeof(DateTime) && value is DateTime dateTime) {
                property.SetValue(target, dateTime);
                return;
            }
            property.SetValue(target, Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture));
        }
    }
}
