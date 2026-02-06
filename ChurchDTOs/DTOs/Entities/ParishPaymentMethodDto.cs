using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ChurchDTOs.DTOs.Utils;

namespace ChurchDTOs.DTOs.Entities
{
    public class ParishPaymentMethodDto : IParishEntity
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; }

        [Required(ErrorMessage = "PaymentMethodId is required for updates.")]
        public int PaymentMethodId { get; set; }

        [Required(ErrorMessage = "ParishId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ParishId must be a positive integer.")]
        public int ParishId { get; set; }

        [Required(ErrorMessage = "MethodType is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "MethodType must be between 1 and 50 characters.")]
        [RegularExpression("^(UPI|BANK|QR|GATEWAY)$", ErrorMessage = "MethodType must be one of: UPI, BANK, QR, GATEWAY")]
        public string MethodType { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "UpiId must not exceed 255 characters.")]
        public string? UpiId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "BankId must be a positive integer if provided.")]
        public int? BankId { get; set; }

        [Required(ErrorMessage = "DisplayName is required.")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "DisplayName must be between 1 and 255 characters.")]
        public string DisplayName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
    }
}
