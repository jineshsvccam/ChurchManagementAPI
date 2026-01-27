using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ChurchDTOs.DTOs.Utils;

namespace ChurchDTOs.DTOs.Entities
{
    public class UnitDto:IParishEntity
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; } // INSERT or UPDATE

        public int UnitId { get; set; }

        [Required(ErrorMessage = "ParishId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ParishId must be a positive integer.")]
        public int ParishId { get; set; }

        [Required(ErrorMessage = "UnitName is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "UnitName must be between 1 and 100 characters.")]
        public string? UnitName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "UnitPresident cannot exceed 100 characters.")]
        public string? UnitPresident { get; set; }

        [StringLength(100, ErrorMessage = "UnitSecretary cannot exceed 100 characters.")]
        public string? UnitSecretary { get; set; }
    }
    public class UnitSimpleDto
    {
        public int UnitId { get; set; }
        public int ParishId { get; set; }
        public string UnitName { get; set; }
        public List<FamilySimpleDto> Families { get; set; }
    }
    public class UnitBasicDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; }
    }

}
