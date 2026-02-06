using System.ComponentModel.DataAnnotations;

namespace ChurchDTOs.DTOs.Entities
{
    public class MemberPaymentApprovalDto
    {
        public Guid? PaymentId { get; set; }

        [Required(ErrorMessage = "ReceiptId is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "ReceiptId must be exactly 6 characters.")]
        public string ReceiptId { get; set; } = string.Empty;

        [Required(ErrorMessage = "ParishId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ParishId must be a positive integer.")]
        public int ParishId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(APPROVED|REJECTED)$", ErrorMessage = "Status must be one of: APPROVED, REJECTED")]
        public string Status { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Remarks must not exceed 1000 characters.")]
        public string? Remarks { get; set; }
    }
}
