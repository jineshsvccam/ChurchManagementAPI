using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchManagementAPI.Controllers.Admin
{
    [ApiController]
    [Route("[controller]")]
    [Authorize] // Require authentication for all actions in this controller
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] RoleDto role)
        {
            var createdRole = await _roleService.AddRoleAsync(role);
            return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.Id }, createdRole);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] RoleDto role)
        {
            if (id != role.Id)
            {
                return BadRequest();
            }
            var updatedRole = await _roleService.UpdateRoleAsync(role);
            if (updatedRole == null)
            {
                return NotFound();
            }
            return Ok(updatedRole);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            await _roleService.DeleteRoleAsync(id);
            return NoContent();
        }
    }
}
