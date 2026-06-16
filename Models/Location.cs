using System.ComponentModel.DataAnnotations;

namespace DSM.Models {
    public class Location {
        public int Id { get; set; }
        [Required(ErrorMessage = "name must be filled.")]
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}