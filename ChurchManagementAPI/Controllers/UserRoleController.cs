using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;

        public UserRoleController(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

        [HttpGet("{userId}/{roleId}")]
        public async Task<IActionResult> GetUserRoleById(Guid userId, Guid roleId)
        {
            var userRole = await _userRoleService.GetUserRoleByIdAsync(userId, roleId);
            if (userRole == null)
            {
                return NotFound();
            }
            return Ok(userRole);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserRole([FromBody] UserRole userRole)
        {
            var createdUserRole = await _userRoleService.AddUserRoleAsync(userRole);
            return CreatedAtAction(nameof(GetUserRoleById), new { userId = createdUserRole.UserId, roleId = createdUserRole.RoleId }, createdUserRole);
        }

        [HttpDelete("{userId}/{roleId}")]
        public async Task<IActionResult> DeleteUserRole(Guid userId, Guid roleId)
        {
            await _userRoleService.DeleteUserRoleAsync(userId, roleId);
            return NoContent();
        }
    }
}
