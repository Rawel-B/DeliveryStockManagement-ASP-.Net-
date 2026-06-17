using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSM.Models {
    public class Shipping {
        public int Id { get; set; }
        [Required(ErrorMessage = "the order must be specified.")]
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        [Required(ErrorMessage = "carrier must be specified.")]
        public int? CarrierId { get; set; }
        public Carrier? Carrier { get; set; }
        [Required(ErrorMessage = "delivery date must be specified.")]
        public DateTime? DeliveryDate { get; set; }
        public DateTime? ReceiptDate { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "cost cannot be negative.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; } = 0m;
        public ShippingStatus Status { get; set; } = ShippingStatus.inPerparation;
        [Required(ErrorMessage = "shipping address must be filled.")]
        [StringLength(250, MinimumLength = 3)]
        public string ShippingAddress { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public string? Remark { get; set; }
        [NotMapped]
        public string? OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum ShippingStatus {
        inPerparation,
        shipped,
        inTransit,
        delivered,
        failed,
        returned
    }
}
