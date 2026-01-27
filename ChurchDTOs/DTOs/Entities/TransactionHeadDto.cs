using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ChurchDTOs.DTOs.Utils;

namespace ChurchDTOs.DTOs.Entities
{
    public class TransactionHeadDto : IParishEntity
    {
        // Used for request operations; remove from response if not set.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; }

        public int HeadId { get; set; }

        [Required(ErrorMessage = "HeadName is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "HeadName must be between 1 and 100 characters.")]
        public string HeadName { get; set; } = string.Empty;

        [AllowedValues("Income", "Expense", "Both", ErrorMessage = "Type must be one of: 'Income', 'Expense', 'Both'.")]
        [StringLength(10, ErrorMessage = "Type cannot exceed 10 characters.")]
        public string? Type { get; set; }

        public bool IsMandatory { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "ParishId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ParishId must be a positive integer.")]
        public int ParishId { get; set; }

        [Range(0, 100, ErrorMessage = "Aramanapct must be between 0 and 100.")]
        public double? Aramanapct { get; set; }

        [StringLength(10, ErrorMessage = "Ordr cannot exceed 10 characters.")]
        public string? Ordr { get; set; }

        [StringLength(100, ErrorMessage = "HeadNameMl cannot exceed 100 characters.")]
        public string? HeadNameMl { get; set; }
    }

    public class TransactionHeadBasicDto
    {
        public int HeadId { get; set; }
        public string HeadName { get; set; } = string.Empty;
        public string? Type { get; set; }
        public double? Aramanapct { get; set; }
        public string? Ordr { get; set; }
        public bool IsMandatory { get; set; }
    }
}
