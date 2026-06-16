namespace DSM.Models {
    public class User {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Name { get; set; }
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