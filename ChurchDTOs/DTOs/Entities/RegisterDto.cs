using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChurchDTOs.DTOs.Entities
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        public string? PhoneNumber { get; set; }

        [Required]
        public List<Guid> RoleIds { get; set; } // Allow assigning multiple roles

        [Required]
        public int ParishId { get; set; }

        [Required]
        public int? FamilyId { get; set; }
    }
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    public class ApproveRoleDto
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }

        public string Status { get; set; } // "Approved" or "Rejected"
    }
    public class AuthResultDto
    {
        public bool IsSuccess { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
        public string FullName { get; set; }
        public int? ParishId { get; set; }
        public string ParishName { get; set; }
        public int? FamilyId { get; set; }
        public string FamilyName { get; set; }
        public List<string> Roles { get; set; }
    }

    public class PendingUserRoleDto
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public int ParishId { get; set; }
        public int FamilyId { get; set; }
        public string RoleName { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string ParishName { get; set; }
        public string FamilyName { get; set; }
        public DateTime RequestedAt { get; set; }
    }

}
