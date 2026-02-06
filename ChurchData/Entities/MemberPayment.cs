using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChurchData
{
    public class MemberPayment
    {
        public int PaymentId { get; set; }
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
        public DateTime ReceivedAt { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        [JsonIgnore]
        public Parish? Parish { get; set; }

        [JsonIgnore]
        public Family? Family { get; set; }

        [JsonIgnore]
        public FamilyMember? Member { get; set; }

        [JsonIgnore]
        public TransactionHead? Head { get; set; }

        [JsonIgnore]
        public ParishPaymentMethod? PaymentMethod { get; set; }

        [JsonIgnore]
        public Bank? Bank { get; set; }

        [JsonIgnore]
        public User? CreatedByUser { get; set; }
    }
}
