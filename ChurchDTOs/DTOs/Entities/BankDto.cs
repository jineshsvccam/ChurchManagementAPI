using System.Text.Json.Serialization;

namespace ChurchDTOs.DTOs.Entities
{

    public class BankDto
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; }
        public int BankId { get; set; }
        public string BankName { get; set; } 
        public string AccountNumber { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public int ParishId { get; set; }
    }
}
