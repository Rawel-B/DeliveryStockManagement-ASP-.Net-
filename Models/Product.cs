using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSM.Models {
    public class Product {
        public int Id { get; set; }
        [Required(ErrorMessage = "the order must be specified.")]
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        [Required(ErrorMessage = "the product is mandatory.")]
        public string ProductName { get; set; } = string.Empty;
        public string? ProductRef { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "cannot enter a quantity less than 1.")]
        public int Quantity { get; set; } = 1;
        [Range(0.01, double.MaxValue, ErrorMessage = "price per unit cannot be less than zero.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerUnit { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public void CalculateSubTotal() {
            SubTotal = PricePerUnit * Quantity;
        }
    }
}
