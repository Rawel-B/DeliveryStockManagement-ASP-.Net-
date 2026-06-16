using DSM.Models;
using Microsoft.EntityFrameworkCore;

namespace DSM.Data {
    public class ApplicationDatabaseContext : DbContext {

        public ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options) : base(options) { 

        }
        public DbSet<Carrier> Carriers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Shipping> Shippings { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<User> Users { get; set; }

    }
}