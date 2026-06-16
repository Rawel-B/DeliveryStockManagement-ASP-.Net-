using System.Globalization;
using DSM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public static class DsmControllerUtilities {
    public static string Clean(string? value) {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public static string? CleanNullable(string? value) {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static string ProductKey(string? value) {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }

    public static void StampNew(dynamic entity) {
        DateTime now = DateTime.Now;
        try { if (entity.CreatedAt == default(DateTime)) entity.CreatedAt = now; } catch { }
        try { entity.UpdatedAt = now; } catch { }
    }

    public static void StampUpdate(dynamic entity) {
        try { entity.UpdatedAt = DateTime.Now; } catch { }
    }

    public static IActionResult ReturnWithError(Controller controller, object model, string message) {
        controller.ModelState.AddModelError(string.Empty, message);
        return controller.View(model);
    }

    public static decimal SafeAmount(decimal? value) {
        return value ?? 0m;
    }

    public static int CurrentUserId(Controller controller) {
        string? raw = controller.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ? id : 0;
    }

    public static string CurrentUsername(Controller controller) {
        return controller.User.Identity?.Name ?? string.Empty;
    }
}
