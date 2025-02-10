using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace ChurchData
{
    public class User : IdentityUser<int>
    {
        public int? ParishId { get; set; }
        public int? FamilyId { get; set; }

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
        public virtual Family Family { get; set; }
        public virtual Parish Parish { get; set; }
    }

    // Custom UserRole: Inherits from IdentityUserRole<int>
    public class UserRole : IdentityUserRole<int>
    {
        // No need to redeclare UserId and RoleId unless additional behavior is needed.
        // Navigation properties
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }

    // Custom Role: Inherits from IdentityRole<int> (which already contains Id and Name)
    public class Role : IdentityRole<int>
    {
        // Remove duplicate properties: Use Id (for role_id) and Name (for role_name)
        // Navigation property
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
        public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    }
}
