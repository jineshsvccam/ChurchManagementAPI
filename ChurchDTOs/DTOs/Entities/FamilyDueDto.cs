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
    public class FamilyDueDto : IParishEntity
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; }

        public int DuesId { get; set; }

        [Required(ErrorMessage = "FamilyId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "FamilyId must be a positive integer.")]
        public int FamilyId { get; set; }

        [Required(ErrorMessage = "HeadId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "HeadId must be a positive integer.")]
        public int HeadId { get; set; }

        [Required(ErrorMessage = "ParishId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ParishId must be a positive integer.")]
        public int ParishId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "OpeningBalance must be a non-negative value.")]
        public decimal OpeningBalance { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "CurrentBalance must be a non-negative value.")]
        public decimal CurrentBalance { get; set; }
    }
}
