using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSM.Models {
    public class Shipping {
        public int Id { get; set; }
        [Required(ErrorMessage = "the order must be specified.")]
        public int OrderId { get; set; }
        public int? CarrierId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime ReceiptDate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; } = 0m;
        public ShippingStatus Status { get; set; } = ShippingStatus.inPerparation;
        public string? ShippingAddress { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Remark { get; set; }
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