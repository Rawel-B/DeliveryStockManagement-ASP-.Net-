using System.ComponentModel.DataAnnotations;

namespace DSM.ViewModels {
    public class SignInViewModel {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public bool Remember { get; set; } = true;
        public bool LoginFailed { get; set; }
        [EmailAddress, StringLength(120)]
        public string? TicketEmail { get; set; }
        public string TicketCategory { get; set; } = "access";
        [StringLength(2000)]
        public string? TicketDescription { get; set; }
        public string? TicketMessage { get; set; }
    }

    public class SignUpViewModel {
        [Required, StringLength(80, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = string.Empty;
        [Required, StringLength(30, MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9._-]+$")]
        public string Username { get; set; } = string.Empty;
        [Required, StringLength(72, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordViewModel {
        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? Error { get; set; }
        [EmailAddress, StringLength(120)]
        public string? TicketEmail { get; set; }
        public string TicketCategory { get; set; } = "access";
        [StringLength(2000, MinimumLength = 10)]
        public string? TicketDescription { get; set; }
        public string? TicketMessage { get; set; }
    }

    public class ProfileViewModel {
        public string? Id { get; set; }
        [Required, StringLength(30, MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9._-]+$")]
        public string Username { get; set; } = string.Empty;
        [Required, StringLength(80, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = string.Empty;
        public string? Role { get; set; }
        public bool IsActive { get; set; }
        [StringLength(72, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        public string ProfileIcon { get; set; } = "pi-user";
    }
}
