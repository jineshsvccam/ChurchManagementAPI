using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ChurchDTOs.DTOs.Utils;

namespace ChurchDTOs.DTOs.Entities
{
    public class FamilyDto : IParishEntity
    {
        /// <summary>
        /// Indicates the operation to be performed ("INSERT" or "UPDATE").
        /// </summary>
        // Only include Action in the request if provided; omit it from the response when null.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; }

        public int FamilyId { get; set; }

        [Required(ErrorMessage = "UnitId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "UnitId must be a positive integer.")]
        public int UnitId { get; set; }

        [Required(ErrorMessage = "FamilyName is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "FamilyName must be between 1 and 100 characters.")]
        public string FamilyName { get; set; } = string.Empty;

        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "ContactInfo cannot exceed 100 characters.")]
        public string? ContactInfo { get; set; }

        [AllowedValues("Low", "Middle", "High", ErrorMessage = "Category must be one of: 'Low', 'Middle', 'High'.")]
        [StringLength(10, ErrorMessage = "Category cannot exceed 10 characters.")]
        public string? Category { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "FamilyNumber must be a positive integer.")]
        public int FamilyNumber { get; set; }

        [AllowedValues("Live", "Left", "Dead", ErrorMessage = "Status must be one of: 'Live', 'Left', 'Dead'.")]
        [StringLength(10, ErrorMessage = "Status cannot exceed 10 characters.")]
        public string? Status { get; set; }

        [Required(ErrorMessage = "HeadName is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "HeadName must be between 1 and 50 characters.")]
        public string HeadName { get; set; } = string.Empty;

        [Required(ErrorMessage = "ParishId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ParishId must be a positive integer.")]
        public int ParishId { get; set; }

        public DateTime? JoinDate { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public decimal? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
        public decimal? Longitude { get; set; }

        public bool HasFamilyPhoto { get; set; }
        public Guid? FamilyPhotoFileId { get; set; }
    }

    public class FamilySimpleDto
    {
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;     
        public int FamilyNumber { get; set; }
        public string? Status { get; set; }
        public string HeadName { get; set; } = string.Empty;
        public int ParishId { get; set; }

        // Geolocation values
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
       
    }
    public class FamilyBasicDto
    {
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public int FamilyNumber { get; set; }
        public int UnitId { get; set; }

        // Geolocation values
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

    }
}

