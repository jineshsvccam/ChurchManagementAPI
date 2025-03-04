using System.Text.Json.Serialization;
using ChurchDTOs.DTOs.Utils;

namespace ChurchDTOs.DTOs.Entities
{
    public class RecurringTransactionDto : IParishEntity
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; }
        public int RepeatedEntryId { get; set; }
        public int HeadId { get; set; }
        public int FamilyId { get; set; }
        public int ParishId { get; set; }
        public decimal IncomeAmount { get; set; }
    }
}
