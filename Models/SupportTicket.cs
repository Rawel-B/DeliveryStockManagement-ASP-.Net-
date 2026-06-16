namespace DSM.Models {
    public class SupportTicket {
        public int Id { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
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