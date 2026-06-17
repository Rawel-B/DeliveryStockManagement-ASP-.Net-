using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using DSM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public static string DisplayLabel(object? value) {
        if (value == null) {
            return string.Empty;
        }

        string raw = value.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(raw)) {
            return string.Empty;
        }

        return raw
            .Replace("_", " ")
            .Replace("-", " ")
            .Aggregate(string.Empty, (text, current) => {
                if (text.Length > 0 && char.IsUpper(current) && char.IsLower(text[^1])) {
                    text += " ";
                }
                return text + current;
            })
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant())
            .Aggregate(string.Empty, (text, word) => string.IsNullOrWhiteSpace(text) ? word : text + " " + word);
    }

    public static List<SelectListItem> EnumSelectList<TEnum>(TEnum selected) where TEnum : struct, Enum {
        return Enum.GetValues<TEnum>()
            .Select(value => new SelectListItem {
                Value = value.ToString(),
                Text = DisplayLabel(value),
                Selected = EqualityComparer<TEnum>.Default.Equals(value, selected)
            })
            .ToList();
    }

    public static string StatusPillClass(object? value) {
        string raw = value?.ToString() ?? string.Empty;
        return raw is "cancelled" or "failed" or "returned" or "refunded" or "closed" or "inactive" ? "status-pill cancelled" : "status-pill";
    }

    public static string PhaseStepClass<TEnum>(TEnum current, TEnum step, IReadOnlyList<TEnum> workflow) where TEnum : struct, Enum {
        int currentIndex = IndexOf(workflow, current);
        int stepIndex = IndexOf(workflow, step);
        if (currentIndex < 0 || stepIndex < 0) {
            return "phase-step";
        }
        if (currentIndex == stepIndex) {
            return "phase-step active";
        }
        if (currentIndex > stepIndex) {
            return "phase-step done";
        }
        return "phase-step";
    }

    private static int IndexOf<TEnum>(IReadOnlyList<TEnum> workflow, TEnum value) where TEnum : struct, Enum {
        for (int index = 0; index < workflow.Count; index++) {
            if (EqualityComparer<TEnum>.Default.Equals(workflow[index], value)) {
                return index;
            }
        }
        return -1;
    }

    public static IReadOnlyList<OrderStatus> OrderWorkflowStatuses() {
        return new[] { OrderStatus.pendingApproval, OrderStatus.validated, OrderStatus.ongoing, OrderStatus.delivered };
    }

    public static IReadOnlyList<ShippingStatus> ShippingWorkflowStatuses() {
        return new[] { ShippingStatus.inPerparation, ShippingStatus.shipped, ShippingStatus.inTransit, ShippingStatus.delivered };
    }

    public static IReadOnlyList<InvoiceStatus> InvoiceWorkflowStatuses() {
        return new[] { InvoiceStatus.pending, InvoiceStatus.processing, InvoiceStatus.completed };
    }

    public static OrderStatus? NextOrderStatus(OrderStatus status) {
        return status == OrderStatus.pendingApproval ? OrderStatus.validated : null;
    }

    public static ShippingStatus? NextShippingStatus(ShippingStatus status) {
        return status switch {
            ShippingStatus.inPerparation => ShippingStatus.shipped,
            ShippingStatus.shipped => ShippingStatus.inTransit,
            ShippingStatus.inTransit => ShippingStatus.delivered,
            _ => null
        };
    }

    public static InvoiceStatus? NextInvoiceStatus(InvoiceStatus status) {
        return status switch {
            InvoiceStatus.pending => InvoiceStatus.processing,
            InvoiceStatus.processing => InvoiceStatus.completed,
            _ => null
        };
    }

    public static string OrderNextActionLabel(OrderStatus status) {
        return status switch {
            OrderStatus.pendingApproval => "Validate Order",
            OrderStatus.validated => "Create Delivery",
            _ => string.Empty
        };
    }

    public static string OrderNextActionIcon(OrderStatus status) {
        return status switch {
            OrderStatus.pendingApproval => "pi pi-check",
            OrderStatus.validated => "pi pi-truck",
            _ => "pi pi-arrow-right"
        };
    }

    public static string ShippingNextActionLabel(ShippingStatus status) {
        return status switch {
            ShippingStatus.inPerparation => "Mark Shipped",
            ShippingStatus.shipped => "Mark In Transit",
            ShippingStatus.inTransit => "Mark Delivered",
            _ => string.Empty
        };
    }

    public static string ShippingNextActionIcon(ShippingStatus status) {
        return status switch {
            ShippingStatus.inPerparation => "pi pi-send",
            ShippingStatus.shipped => "pi pi-truck",
            ShippingStatus.inTransit => "pi pi-check-circle",
            _ => "pi pi-arrow-right"
        };
    }

    public static string InvoiceNextActionLabel(InvoiceStatus status) {
        return status switch {
            InvoiceStatus.pending => "Start Processing",
            InvoiceStatus.processing => "Mark Completed",
            _ => string.Empty
        };
    }

    public static string InvoiceNextActionIcon(InvoiceStatus status) {
        return status switch {
            InvoiceStatus.pending => "pi pi-hourglass",
            InvoiceStatus.processing => "pi pi-check-circle",
            _ => "pi pi-arrow-right"
        };
    }

    public static bool OrderCanCancel(OrderStatus status) {
        return status is OrderStatus.pendingApproval or OrderStatus.validated;
    }

    public static bool ShippingCanFail(ShippingStatus status) {
        return status is ShippingStatus.inPerparation or ShippingStatus.shipped or ShippingStatus.inTransit;
    }

    public static bool ShippingCanReturn(ShippingStatus status) {
        return status is ShippingStatus.shipped or ShippingStatus.inTransit or ShippingStatus.failed;
    }

    public static bool InvoiceCanFail(InvoiceStatus status) {
        return status == InvoiceStatus.processing;
    }

    public static bool InvoiceCanRefund(InvoiceStatus status) {
        return status == InvoiceStatus.completed;
    }

    public static bool InvoiceCanCancel(InvoiceStatus status) {
        return status is InvoiceStatus.pending or InvoiceStatus.processing or InvoiceStatus.failed;
    }

}
