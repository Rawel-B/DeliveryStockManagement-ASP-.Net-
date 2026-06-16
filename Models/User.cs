using System.ComponentModel.DataAnnotations;

namespace DSM.Models {
    public class User {
        public int Id { get; set; }
        [StringLength(30, MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9._-]+$", ErrorMessage = "username format is invalid.")]
        public string? Username { get; set; }
        public string? Password { get; set; }
        [StringLength(80, MinimumLength = 2)]
        public string? Name { get; set; }
        [EmailAddress]
        [StringLength(120)]
        public string? Email { get; set; }
        public Role Role { get; set; } = Role.user;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public enum Role {
        administrator,
        manager,
        user
    }
}
