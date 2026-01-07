using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData.Entities
{
    [Table("family_files")]
    public class FamilyFile
    {
        [Key]
        [Column("file_id")]
        public Guid FileId { get; set; }

        [Column("family_id")]
        public int FamilyId { get; set; }

        [Column("member_id")]
        public int? MemberId { get; set; }

        [Required]
        [Column("file_category")]
        [MaxLength(30)]
        public string FileCategory { get; set; } = null!;

        [Required]
        [Column("file_type")]
        [MaxLength(30)]
        public string FileType { get; set; } = null!;

        [Required]
        [Column("file_key")]
        public string FileKey { get; set; } = null!;

        [Column("is_primary")]
        public bool IsPrimary { get; set; }

        [Column("status")]
        [MaxLength(10)]
        public string Status { get; set; } = "Pending";

        [Column("uploaded_by")]
        public Guid? UploadedBy { get; set; }

        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; }

        // 🔗 Navigation properties (optional but recommended)
        public Family? Family { get; set; }
        public FamilyMember? FamilyMember { get; set; }
    }
}
