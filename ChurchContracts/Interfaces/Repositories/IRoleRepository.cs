using ChurchData;
using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IRoleRepository
    {
        Task<Role> GetRoleByIdAsync(Guid roleId);
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> AddRoleAsync(RoleDto role);
        Task<RoleDto> UpdateRoleAsync(RoleDto role);
        Task DeleteRoleAsync(Guid roleId);
    }
}
