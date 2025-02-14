using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChurchData
{

    public class PendingFamilyMemberAction
    {
        [Key]
        public int ActionId { get; set; }

        [Required]
        public int FamilyId { get; set; }

        [Required]
        public int ParishId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        public int SubmittedBy { get; set; }

        [Required, MaxLength(10)]
        public string ActionType { get; set; }  // e.g. "INSERT"

        [Required]
        public JsonElement SubmittedData { get; set; }  // Stored as JSON

        [Required, MaxLength(20)]
        public string ApprovalStatus { get; set; } = "Pending";  // Maps to approval_status

        [Required]
        public DateTime SubmittedAt { get; set; }  // Maps to submitted_at

        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}


