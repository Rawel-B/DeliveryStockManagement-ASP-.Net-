using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSM.Models {
    public class Carrier {
        public int Id { get; set; }
        [Required(ErrorMessage = "name must be filled.")]
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        [Range(0.0, 5.0, ErrorMessage = "rating minimum is 0, rating maximum is 5")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Rating { get; set; } = 0m;
        public bool IsActive { get; set; } = true;
        public List<Shipping> Shippings { get; set; } = new();
        [NotMapped]
        public List<string> ShippingIds { get; set; } = new();
        [NotMapped]
        public int ShippingsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
