using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ChurchDTOs.DTOs.Utils;

namespace ChurchDTOs.DTOs.Entities
{
    public class ContributionSettingsDto : IParishEntity
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; }

        public int SettingId { get; set; }

        [Required(ErrorMessage = "HeadId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "HeadId must be a positive integer.")]
        public int HeadId { get; set; }

        [Required(ErrorMessage = "ParishId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ParishId must be a positive integer.")]
        public int ParishId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a non-negative value.")]
        public decimal Amount { get; set; }

        [AllowedValues("Monthly", "Annually", ErrorMessage = "Frequency must be one of: 'Monthly', 'Annually'.")]
        [StringLength(10, ErrorMessage = "Frequency cannot exceed 10 characters.")]
        public string? Frequency { get; set; }

        [Range(1, 31, ErrorMessage = "DueDay must be between 1 and 31.")]
        public int? DueDay { get; set; }

        [Range(1, 12, ErrorMessage = "DueMonth must be between 1 and 12.")]
        public int? DueMonth { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "FineAmount must be a non-negative value.")]
        public decimal? FineAmount { get; set; }

        [Required(ErrorMessage = "ValidFrom is required.")]
        public DateTime ValidFrom { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [AllowedValues("Low", "Middle", "High", ErrorMessage = "Category must be one of: 'Low', 'Middle', 'High'.")]
        [StringLength(10, ErrorMessage = "Category cannot exceed 10 characters.")]
        public string Category { get; set; } = string.Empty;
    }
}
