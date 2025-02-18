using System.Text.Json.Serialization;

namespace ChurchDTOs.DTOs.Entities
{

    public class BankDto
    {
        // Only include Action in the request if provided; omit it from the response when null.
        [JsonIgnore]
        public string Action { get; set; } = string.Empty;
        public int BankId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public int ParishId { get; set; }
    }
}
