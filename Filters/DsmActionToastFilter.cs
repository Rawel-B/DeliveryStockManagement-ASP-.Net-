using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DSM.Filters {
    public class DsmActionToastFilter : IActionFilter {
        public void OnActionExecuting(ActionExecutingContext context) {
        }

        public void OnActionExecuted(ActionExecutedContext context) {
            if (!string.Equals(context.HttpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            if (context.Exception != null) {
                return;
            }

            bool redirected = context.Result is RedirectToActionResult || context.Result is RedirectResult || context.Result is LocalRedirectResult;
            if (!redirected) {
                return;
            }

            if (context.Controller is not Controller controller) {
                return;
            }

            if (controller.TempData.ContainsKey("ToastMessage") || controller.TempData.ContainsKey("AuthMessage") || controller.TempData.ContainsKey("ProfileMessage") || controller.TempData.ContainsKey("Error")) {
                return;
            }

            string module = Humanize(context.RouteData.Values["controller"]?.ToString());
            string action = context.RouteData.Values["action"]?.ToString() ?? string.Empty;
            string message = BuildMessage(module, action);

            if (string.IsNullOrWhiteSpace(message)) {
                return;
            }

            controller.TempData["ToastMessage"] = message;
            controller.TempData["ToastType"] = "success";
        }

        private static string BuildMessage(string module, string action) {
            if (string.IsNullOrWhiteSpace(module)) {
                module = "Record";
            }

            return action switch {
                "Create" => module + " created successfully.",
                "Edit" => module + " updated successfully.",
                "Delete" => module + " deleted successfully.",
                "DeleteConfirmed" => module + " deleted successfully.",
                "ValidateOrder" => "Order validated successfully.",
                "SetOrderStatus" => "Order status updated successfully.",
                "SetShippingStatus" => "Delivery status updated successfully.",
                "SetInvoiceStatus" => "Invoice status updated successfully.",
                "AssignTicket" => "Support ticket assigned successfully.",
                "ResolveTicket" => "Support ticket resolved successfully.",
                "SignOut" => "Signed out.",
                "SignOutUser" => "Signed out.",
                _ => string.Empty
            };
        }

        private static string Humanize(string? value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return string.Empty;
            }

            return value switch {
                "Shipping" => "Delivery",
                "SupportTicket" => "Support ticket",
                _ => value
            };
        }
    }
}
