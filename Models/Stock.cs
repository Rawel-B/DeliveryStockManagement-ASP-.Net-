using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSM.Models {
    public class Stock {
        public int Id { get; set; }
        [Required(ErrorMessage = "the product must be specified.")]
        public string Product { get; set; } = string.Empty;
        [Required(ErrorMessage = "product reference must be filled.")]
        public string ProductRef { get; set; } = string.Empty;
        [Required(ErrorMessage = "location must be specified.")]
        public int? LocationId { get; set; }
        public Location? LocationEntity { get; set; }
        public string? Location { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "quantity cannot be less than 0.")]
        public int Quantity { get; set; } = 0;
        public DateTime LastReceiptDate { get; set; } = DateTime.Now;
        [NotMapped]
        public int ReservedQuantity { get; set; }
        [NotMapped]
        public int AvailableQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
