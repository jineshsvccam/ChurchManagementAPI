using System.ComponentModel.DataAnnotations;
using ChurchDTOs.DTOs.Utils;

namespace ChurchDTOs.DTOs.Entities
{
    public class MemberPaymentBulkItemDto : IParishEntity
    {
        [Required(ErrorMessage = "Action is required.")]
        [RegularExpression("^(INSERT|UPDATE)$", ErrorMessage = "Action must be one of: INSERT, UPDATE")]
        public string Action { get; set; } = string.Empty;

        public Guid PaymentId { get; set; }

        [Required(ErrorMessage = "ParishId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ParishId must be a positive integer.")]
        public int ParishId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "FamilyId must be a positive integer.")]
        public int FamilyId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MemberId must be a positive integer if provided.")]
        public int? MemberId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "HeadId must be a positive integer.")]
        public int HeadId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "PaymentMethodId must be a positive integer.")]
        public int PaymentMethodId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "BankId must be a positive integer if provided.")]
        public int? BankId { get; set; }

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
