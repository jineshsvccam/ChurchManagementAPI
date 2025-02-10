using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChurchData.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public List<int> RoleIds { get; set; } // Allow assigning multiple roles
    }
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

   
}
