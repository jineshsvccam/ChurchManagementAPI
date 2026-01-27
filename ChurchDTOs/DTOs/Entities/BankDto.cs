using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ChurchDTOs.DTOs.Utils;

namespace ChurchDTOs.DTOs.Entities
{

    public class BankDto : IParishEntity
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; }

        public int BankId { get; set; }

        [Required(ErrorMessage = "BankName is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "BankName must be between 1 and 100 characters.")]
        public string BankName { get; set; } = string.Empty;

        [Required(ErrorMessage = "AccountNumber is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "AccountNumber must be between 1 and 50 characters.")]
        public string AccountNumber { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "OpeningBalance must be a non-negative value.")]
        public decimal OpeningBalance { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "CurrentBalance must be a non-negative value.")]
        public decimal CurrentBalance { get; set; }

        [Required(ErrorMessage = "ParishId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ParishId must be a positive integer.")]
        public int ParishId { get; set; }
    }

    public class BankBasicDto
    {
        public int BankId { get; set; }
        public string BankName { get; set; } = string.Empty;
    }
}
