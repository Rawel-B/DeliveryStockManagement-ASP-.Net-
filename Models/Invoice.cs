using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSM.Models {
    public class Invoice {
        public int Id { get; set; }
        [Required(ErrorMessage = "the order must be specified.")]
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public DateTime InvoicingDate { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.pending;
        [Required(ErrorMessage = "must add an invoicing method.")]
        public InvoicingMethod Method { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "total amount cannot be negative.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string? TransactionRef { get; set; }
        public string? Remark { get; set; }
        [NotMapped]
        public string? OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public enum InvoiceStatus {
        pending,
        processing,
        completed,
        failed,
        refunded,
        cancelled
    }

    public enum InvoicingMethod {
        creditCard,
        debitCard,
        bankTransfer,
        Check,
        Cash,
        paypal,
        stripe,
        other
    }
}
