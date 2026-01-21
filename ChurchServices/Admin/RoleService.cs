using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchServices.Admin
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<Role> GetRoleByIdAsync(Guid roleId)
        {
            return await _roleRepository.GetRoleByIdAsync(roleId);
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllRolesAsync();
        }

        public async Task<RoleDto> AddRoleAsync(RoleDto role)
        {
            return await _roleRepository.AddRoleAsync(role);
        }

        public async Task<RoleDto> UpdateRoleAsync(RoleDto role)
        {
            return await _roleRepository.UpdateRoleAsync(role);
        }

        public async Task DeleteRoleAsync(Guid roleId)
        {
            await _roleRepository.DeleteRoleAsync(roleId);
        }
    }
}
