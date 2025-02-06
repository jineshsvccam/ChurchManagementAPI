using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchData
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int? ParishId { get; set; }

        // Navigation properties
        public ICollection<UserRole> UserRoles { get; set; }
    }

    public class UserRole
    {
        //public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Role Role { get; set; }
    }

    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        // Navigation properties
        public ICollection<UserRole> UserRoles { get; set; }

    }
}
