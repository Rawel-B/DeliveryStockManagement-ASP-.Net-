using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSM.Models {
    public class Location {
        public int Id { get; set; }
        [Required(ErrorMessage = "name must be filled.")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "code must be filled.")]
        [StringLength(60, MinimumLength = 2)]
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<Stock> Stocks { get; set; } = new();
        [NotMapped]
        public int StockCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
