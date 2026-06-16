using System.ComponentModel.DataAnnotations;

namespace DSM.Models {
    public class Customer {
        public int Id { get; set; }
        [Required(ErrorMessage = "name must be filled.")]
        public string Name { get; set; } = string.Empty;
        [EmailAddress(ErrorMessage = "email must be valid.")]
        [Required(ErrorMessage = "email must be filled.")]
        public string Email { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}