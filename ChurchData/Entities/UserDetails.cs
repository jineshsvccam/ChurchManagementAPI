using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ChurchData
{
    public class User : IdentityUser<Guid>
    {
        [Required]
        [Column("fullname")]
        [MaxLength(255)]
        public string FullName { get; set; }

        [Column("family_id")]
        public int? FamilyId { get; set; }

        [Column("parish_id")]
        public int? ParishId { get; set; }


        public UserStatus Status { get; set; } = UserStatus.Pending;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
        public virtual Family? Family { get; set; }
        public virtual Parish? Parish { get; set; }
    }

    public class UserRole : IdentityUserRole<Guid>
    {
        [Key, Column(Order = 0)]
        public Guid UserId { get; set; }

        [Key, Column(Order = 1)]
        public Guid RoleId { get; set; }

        [Required]
        public RoleStatus Status { get; set; } = RoleStatus.Pending;

        public Guid? ApprovedBy { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }

        public virtual User? User { get; set; }
        public virtual Role? Role { get; set; }
        public virtual User? ApprovedByUser { get; set; }
    }
    public class Role : IdentityRole<Guid>
    {
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    }
    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended,
        Pending
    }

    public enum RoleStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
