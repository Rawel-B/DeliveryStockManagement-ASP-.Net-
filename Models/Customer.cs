using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSM.Models {
    public class Customer {
        public int Id { get; set; }
        [Required(ErrorMessage = "name must be filled.")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        [EmailAddress(ErrorMessage = "email must be valid.")]
        [Required(ErrorMessage = "email must be filled.")]
        public string Email { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public List<Order> Orders { get; set; } = new();
        [NotMapped]
        public int OrdersCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
