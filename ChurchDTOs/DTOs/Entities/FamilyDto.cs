using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChurchDTOs.DTOs.Entities
{
    public class FamilyDto
    {
        /// <summary>
        /// Indicates the operation to be performed ("INSERT" or "UPDATE").
        /// </summary>
        // Only include Action in the request if provided; omit it from the response when null.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Action { get; set; }

        public int FamilyId { get; set; }
        public int UnitId { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? ContactInfo { get; set; }
        public string? Category { get; set; }
        public int FamilyNumber { get; set; }
        public string? Status { get; set; }
        public string HeadName { get; set; } = string.Empty;
        public int ParishId { get; set; }
        public DateTime? JoinDate { get; set; }
    }
}

