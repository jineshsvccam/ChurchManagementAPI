using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace ChurchData
{
    public class User : IdentityUser<int>
    {
        public int? ParishId { get; set; }
        public int? FamilyId { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Pending; // Default status

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
        public virtual Family Family { get; set; }
        public virtual Parish Parish { get; set; }
    }

    // Custom UserRole: Inherits from IdentityUserRole<int>
    public class UserRole : IdentityUserRole<int>
    {
        [Required]
        public RoleStatus Status { get; set; } = RoleStatus.Pending;

        public int? ApprovedBy { get; set; }  // Nullable, since initially unapproved

        [Column(TypeName = "timestamp")]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamp")]
        public DateTime? ApprovedAt { get; set; } // Nullable, as it’s only set upon approval

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
        public virtual User ApprovedByUser { get; set; }  // Admin who approved/rejected the role

    }


    // Custom Role: Inherits from IdentityRole<int> (which already contains Id and Name)
    public class Role : IdentityRole<int>
    {
        // Remove duplicate properties: Use Id (for role_id) and Name (for role_name)
        // Navigation property
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    }
    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended,
        Pending // For newly registered users waiting for activation
    }
    public enum RoleStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
