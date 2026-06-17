using System.ComponentModel.DataAnnotations;

namespace DSM.Models {
    public class SupportTicket {
        public int Id { get; set; }
        [Required(ErrorMessage = "subject must be filled.")]
        [StringLength(120, MinimumLength = 3)]
        public string Subject { get; set; } = string.Empty;
        [Required(ErrorMessage = "description must be filled.")]
        [StringLength(2000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;
        public Category Category { get; set; } = Category.operations;
        public Priority Priority { get; set; } = Priority.normal;
        public Status Status { get; set; } = Status.open;
        public int? RequesterId { get; set; }
        public string? RequesterName { get; set; }
        public string? RequesterEmail { get; set; }
        public int? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum Category {
        operations,
        account,
        data,
        technical,
        access,
        accountActivation
    }

    public enum Priority {
        low,
        normal,
        high,
        urgent
    }

    public enum Status {
        open,
        inProgress,
        resolved,
        closed
    }
}
