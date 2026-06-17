using DSM.Data;
using DSM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DSM.Controllers {
    [Authorize]
    public class DashboardController : Controller {
        private readonly ApplicationDatabaseContext _context;

        public DashboardController(ApplicationDatabaseContext context) {
            _context = context;
        }

        // GET: DASHBOARD
        public async Task<IActionResult> Index() {
            ViewData["Title"] = "Dashboard";
            ViewBag.OrdersCount = await _context.Orders.CountAsync();
            ViewBag.OrdersPendingApproval = await _context.Orders.CountAsync(o => o.Status == OrderStatus.pendingApproval);
            ViewBag.OrdersOngoing = await _context.Orders.CountAsync(o => o.Status == OrderStatus.ongoing);
            ViewBag.OrdersDelivered = await _context.Orders.CountAsync(o => o.Status == OrderStatus.delivered);
            ViewBag.OrdersCancelled = await _context.Orders.CountAsync(o => o.Status == OrderStatus.cancelled);
            ViewBag.Revenue = await _context.Invoices.Where(i => i.Status == InvoiceStatus.completed).SumAsync(i => (decimal?)i.Amount) ?? 0m;
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();
            ViewBag.TotalCarriers = await _context.Carriers.CountAsync();
            ViewBag.TotalSuppliers = await _context.Suppliers.CountAsync();
            ViewBag.TotalStocks = await _context.Stocks.CountAsync();
            ViewBag.TotalLocations = await _context.Locations.CountAsync();
            ViewBag.TotalInvoices = await _context.Invoices.CountAsync();
            ViewBag.TotalShippings = await _context.Shippings.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalSupportTickets = await _context.SupportTickets.CountAsync();
            ViewBag.ShippingInPerparation = await _context.Shippings.CountAsync(s => s.Status == ShippingStatus.inPerparation);
            ViewBag.InvoicePending = await _context.Invoices.CountAsync(i => i.Status == InvoiceStatus.pending);
            return View();
        }
    }
}
