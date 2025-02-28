using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchServices
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<Role> GetRoleByIdAsync(int roleId)
        {
            return await _roleRepository.GetRoleByIdAsync(roleId);
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllRolesAsync();
        }

        public async Task<Role> AddRoleAsync(Role role)
        {
            return await _roleRepository.AddRoleAsync(role);
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            return await _roleRepository.UpdateRoleAsync(role);
        }

        public async Task DeleteRoleAsync(int roleId)
        {
            await _roleRepository.DeleteRoleAsync(roleId);
        }
    }
}
