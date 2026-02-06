using System.ComponentModel.DataAnnotations;
using ChurchDTOs.DTOs.Utils;

namespace ChurchDTOs.DTOs.Entities
{
    public class MemberPaymentDto : IParishEntity
    {
        public Guid PaymentId { get; set; }

        public string ReceiptId { get; set; } = string.Empty;

        public int ParishId { get; set; }

        public int FamilyId { get; set; }

        public int? MemberId { get; set; }

        public int HeadId { get; set; }

        public int PaymentMethodId { get; set; }

        public int? BankId { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMode { get; set; } = string.Empty;

        public string? UtrNumber { get; set; }

        public string? ReferenceNumber { get; set; }

        public DateTime PaymentDate { get; set; }

        public DateTimeOffset ReceivedAt { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTimeOffset? ApprovedAt { get; set; }

        public Guid? ApprovedBy { get; set; }

        public string? Remarks { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public Guid CreatedBy { get; set; }
    }
}
