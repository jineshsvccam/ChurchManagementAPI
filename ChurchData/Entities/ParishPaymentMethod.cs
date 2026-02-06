using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChurchData
{
    public class ParishPaymentMethod
    {
        public int PaymentMethodId { get; set; }
        public int ParishId { get; set; }
        public string MethodType { get; set; } = string.Empty;
        public string? UpiId { get; set; }
        public int? BankId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public Parish? Parish { get; set; }

        [JsonIgnore]
        public Bank? Bank { get; set; }
    }
}
