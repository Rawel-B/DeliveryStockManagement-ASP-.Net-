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

        public async Task<IActionResult> Index() {
            ViewData["Title"] = "Dashboard";
            ViewBag.OrdersCount = await _context.Orders.CountAsync();
            ViewBag.OrdersPendingApproval = await _context.Orders.CountAsync(o => o.Status == OrderStatus.pendingApproval);
            ViewBag.OrdersOngoing = await _context.Orders.CountAsync(o => o.Status == OrderStatus.ongoing);
            ViewBag.OrdersDelivered = await _context.Orders.CountAsync(o => o.Status == OrderStatus.delivered);
            ViewBag.OrdersCancelled = await _context.Orders.CountAsync(o => o.Status == OrderStatus.cancelled);
            ViewBag.Revenue = await _context.Orders.SumAsync(o => o.TotalAmount);
            ViewBag.CustomersCount = await _context.Customers.CountAsync();
            ViewBag.CarriersCount = await _context.Carriers.CountAsync();
            ViewBag.SuppliersCount = await _context.Suppliers.CountAsync();
            ViewBag.StocksCount = await _context.Stocks.CountAsync();
            ViewBag.ShippingInPerparation = await _context.Shippings.CountAsync(s => s.Status == ShippingStatus.inTransit || s.Status == ShippingStatus.shipped);
            ViewBag.InvoicePending = await _context.Invoices.CountAsync(i => i.Status == InvoiceStatus.pending);
            ViewBag.SupportCount = await _context.SupportTickets.CountAsync();
            return View();
        }
    }
}
