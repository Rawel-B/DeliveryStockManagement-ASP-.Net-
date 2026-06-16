using System.ComponentModel.DataAnnotations;

namespace DSM.Models {
    public class Stock {
        public int Id { get; set; }
        [Required(ErrorMessage = "the product must be specified.")]
        public string Product { get; set; } = string.Empty;
        public string? ProductRef { get; set; }
        public int? LocationId { get; set; }
        public string? Location { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "quantity cannot be less than 0.")]
        public int Quantity { get; set; } = 0;
        public DateTime LastReceiptDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}