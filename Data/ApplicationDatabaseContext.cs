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

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasOne(order => order.Customer)
                .WithMany(customer => customer.Orders)
                .HasForeignKey(order => order.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(order => order.Supplier)
                .WithMany(supplier => supplier.Orders)
                .HasForeignKey(order => order.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>()
                .HasOne(product => product.Order)
                .WithMany(order => order.Products)
                .HasForeignKey(product => product.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Shipping>()
                .HasOne(shipping => shipping.Order)
                .WithMany(order => order.Shippings)
                .HasForeignKey(shipping => shipping.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipping>()
                .HasOne(shipping => shipping.Carrier)
                .WithMany(carrier => carrier.Shippings)
                .HasForeignKey(shipping => shipping.CarrierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Invoice>()
                .HasOne(invoice => invoice.Order)
                .WithMany(order => order.Invoices)
                .HasForeignKey(invoice => invoice.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>()
                .HasOne(stock => stock.LocationEntity)
                .WithMany(location => location.Stocks)
                .HasForeignKey(stock => stock.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Username)
                .IsUnique()
                .HasFilter("[Username] IS NOT NULL");

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");
        }

        public override int SaveChanges() {
            StampEntities();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
            StampEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void StampEntities() {
            DateTime now = DateTime.Now;

            foreach (var entry in ChangeTracker.Entries()) {
                if (entry.State == EntityState.Added) {
                    if (entry.Properties.Any(property => property.Metadata.Name == "CreatedAt")) {
                        var property = entry.Property("CreatedAt");

                        if (property.CurrentValue is null || property.CurrentValue is DateTime date && date == default) {
                            property.CurrentValue = now;
                        }
                    }

                    if (entry.Properties.Any(property => property.Metadata.Name == "UpdatedAt")) {
                        entry.Property("UpdatedAt").CurrentValue = now;
                    }
                }

                if (entry.State == EntityState.Modified && entry.Properties.Any(property => property.Metadata.Name == "UpdatedAt")) {
                    entry.Property("UpdatedAt").CurrentValue = now;
                }
            }
        }
    }
}