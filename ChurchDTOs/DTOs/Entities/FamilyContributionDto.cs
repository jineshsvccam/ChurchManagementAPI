using System.Text.Json.Serialization;
using ChurchDTOs.DTOs.Utils;

namespace ChurchDTOs.DTOs.Entities
{
    public class FamilyContributionDto : IParishEntity
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; }
        public int ContributionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string VoucherNumber { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int HeadId { get; set; }
        public int FamilyId { get; set; }
        public int BankId { get; set; }
        public int ParishId { get; set; }
        public int SettingId { get; set; }
        public decimal IncomeAmount { get; set; }
        public decimal ExpenseAmount { get; set; }
        public string? Description { get; set; }
    }
}
