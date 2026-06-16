using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using DSM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DSM.Data {
    public static class AuthSeedExtensions {
        public static async Task SeedDefaultAdministrator(this WebApplication app) {
            using IServiceScope scope = app.Services.CreateScope();
            ApplicationDatabaseContext context = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();
            await context.Database.EnsureCreatedAsync();
            PasswordHasher<User> passwordHasher = new();

            if (!await context.Users.AnyAsync(u => u.Username == "admin")) {
                User admin = new() {
                    Username = "admin",
                    Name = "Administrator",
                    Email = "admin@dsm.com",
                    Role = Role.administrator,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                admin.Password = passwordHasher.HashPassword(admin, "admin123");
                context.Users.Add(admin);
            }

            if (!await context.Users.AnyAsync(u => u.Username == "manager")) {
                User manager = new() {
                    Username = "manager",
                    Name = "Manager",
                    Email = "manager@dsm.com",
                    Role = Role.manager,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                manager.Password = passwordHasher.HashPassword(manager, "manager123");
                context.Users.Add(manager);
            }

            await context.SaveChangesAsync();
        }
    }
}
