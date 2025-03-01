using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
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

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRoles()
        {
            var pendingRoles = await _userRoleService.GetPendingUserRolesAsync();
            return Ok(pendingRoles);
        }

        [HttpPost("approverole")]
        public async Task<IActionResult> ApproveUserRoleAsync([FromBody] ApproveRoleDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid request payload.");
            }

            var result = await _userRoleService.ApproveUserRoleAsync(dto);
            if (result == null)
            {
                return NotFound("User role not found.");
            }

            return Ok(new { message = "User role updated successfully." });
        }

    }
}