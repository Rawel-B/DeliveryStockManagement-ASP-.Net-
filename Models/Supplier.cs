using System.ComponentModel.DataAnnotations;

namespace DSM.Models {
    public class Supplier {
        public int Id { get; set; }
        [Required(ErrorMessage = "name must be filled.")]
        public string Name { get; set; } = string.Empty;
        [EmailAddress(ErrorMessage = "email must be valid.")]
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}