using DSM.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class DashboardController : Controller {
    private readonly ApplicationDatabaseContext _context;

    public DashboardController(ApplicationDatabaseContext context) {
        _context = context;
    }

    // GET: DASHBOARD
    public async Task<IActionResult> Index() {
        ViewBag.OrdersCount = await _context.Orders.CountAsync();
        ViewBag.TotalCustomers = await _context.Customers.CountAsync();
        ViewBag.TotalCarriers = await _context.Carriers.CountAsync();
        ViewBag.TotalSuppliers = await _context.Suppliers.CountAsync();
        ViewBag.TotalStocks = await _context.Stocks.CountAsync();
        ViewBag.TotalLocations = await _context.Locations.CountAsync();
        ViewBag.TotalInvoices = await _context.Invoices.CountAsync();
        ViewBag.TotalShippings = await _context.Shippings.CountAsync();
        ViewBag.TotalProducts = await _context.Products.CountAsync();
        ViewBag.TotalUsers = await _context.Users.CountAsync();
        ViewBag.TotalSupportTickets = await _context.SupportTickets.CountAsync();

        return View();
    }
}
