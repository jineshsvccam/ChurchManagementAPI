using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChurchData.DTOs
{
    public class PendingFamilyMemberRequestDto
    {
        [Required]
        public int FamilyId { get; set; }

        [Required]
        public int ParishId { get; set; }       

        [Required]
        public int SubmittedBy { get; set; }

        // The payload property represents the complete JSON data.
        [Required]
        public JsonElement Payload { get; set; }
    }
    public class FamilyMemberApprovalDto
    {
        [Required]
        public int ActionId { get; set; }

        [Required]
        public int ApprovedBy { get; set; }
    }

    public class ServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

}
