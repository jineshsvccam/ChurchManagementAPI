using System.Text.Json.Serialization;

namespace ChurchDTOs.DTOs.Entities
{
    public class TransactionHeadDto
    {
        // Used for request operations; remove from response if not set.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; }

        public int HeadId { get; set; }
        public string HeadName { get; set; } = string.Empty;
        public string? Type { get; set; }
        public bool IsMandatory { get; set; }
        public string? Description { get; set; }
        public int ParishId { get; set; }
        public double? Aramanapct { get; set; }
        public string? Ordr { get; set; }
        public string? HeadNameMl { get; set; }
    }

}
