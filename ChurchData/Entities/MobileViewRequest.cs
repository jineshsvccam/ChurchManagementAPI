using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChurchData.Entities
{
    /// <summary>
    /// Tracks mobile number view requests for rate limiting (max 5 per day per user)
    /// </summary>
    [Table("mobile_view_requests")]
    public class MobileViewRequest
    {
        [Key]
        [Column("request_id")]
        public int RequestId { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("member_id")]
        public int MemberId { get; set; }

        [Required]
        [Column("requested_at")]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("MemberId")]
        public virtual FamilyMember? Member { get; set; }
    }
}
