using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSM.Models {
    public class Supplier {
        public int Id { get; set; }
        [Required(ErrorMessage = "name must be filled.")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "email must be filled.")]
        [EmailAddress(ErrorMessage = "email must be valid.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "phone must be filled.")]
        [StringLength(40, MinimumLength = 3)]
        public string Phone { get; set; } = string.Empty;
        [Required(ErrorMessage = "address must be filled.")]
        [StringLength(250, MinimumLength = 3)]
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public List<Order> Orders { get; set; } = new();
        [NotMapped]
        public int OrdersCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
