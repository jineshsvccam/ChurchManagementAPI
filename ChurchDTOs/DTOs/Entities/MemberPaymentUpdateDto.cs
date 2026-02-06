using System.ComponentModel.DataAnnotations;

namespace ChurchDTOs.DTOs.Entities
{
    public class MemberPaymentUpdateDto
    {
        [Required(ErrorMessage = "PaymentId is required.")]
        public Guid PaymentId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "PaymentMode is required.")]
        [RegularExpression("^(UPI|CASH|BANK|GATEWAY)$", ErrorMessage = "PaymentMode must be one of: UPI, CASH, BANK, GATEWAY")]
        public string PaymentMode { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "UtrNumber must not exceed 50 characters.")]
        public string? UtrNumber { get; set; }

        [StringLength(50, ErrorMessage = "ReferenceNumber must not exceed 50 characters.")]
        public string? ReferenceNumber { get; set; }

        [Required(ErrorMessage = "PaymentDate is required.")]
        public DateTime PaymentDate { get; set; }

        [StringLength(1000, ErrorMessage = "Remarks must not exceed 1000 characters.")]
        public string? Remarks { get; set; }
    }
}
