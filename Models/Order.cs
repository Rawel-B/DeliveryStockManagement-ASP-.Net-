using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSM.Models {
    public class Order {
        public int Id { get; set; }
        [Required(ErrorMessage = "the customer must be specified.")]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        [Required(ErrorMessage = "the supplier must be specified.")]
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.pendingApproval;
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0m;
        public string? OrderNumber { get; set; }
        public string? Remark { get; set; }
        public List<Product> Products { get; set; } = new();
        public List<Shipping> Shippings { get; set; } = new();
        public List<Invoice> Invoices { get; set; } = new();
        [NotMapped]
        public List<string> ShippingIds { get; set; } = new();
        [NotMapped]
        public List<string> InvoiceIds { get; set; } = new();
        [NotMapped]
        public string? CustomerName { get; set; }
        [NotMapped]
        public string? SupplierName { get; set; }
        [NotMapped]
        public int OrdersCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public void Init() {
            if (OrderDate == default) {
                OrderDate = DateTime.Now;
            }
            if (string.IsNullOrWhiteSpace(OrderNumber)) {
                OrderNumber = "CMD-" + DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
        }
    }

    public enum OrderStatus {
        pendingApproval,
        validated,
        ongoing,
        delivered,
        cancelled
    }
}
