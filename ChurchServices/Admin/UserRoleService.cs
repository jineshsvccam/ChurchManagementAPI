using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchServices.Admin
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;

        public UserRoleService(IUserRoleRepository userRoleRepository)
        {
            _userRoleRepository = userRoleRepository;
        }

        public async Task<UserRole> GetUserRoleByIdAsync(Guid userId, Guid roleId)
        {
            return await _userRoleRepository.GetUserRoleByIdAsync(userId, roleId);
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(Guid userId)
        {
            return await _userRoleRepository.GetUserRolesByUserIdAsync(userId);
        }

        public async Task<UserRole> AddUserRoleAsync(UserRole userRole)
        {
            return await _userRoleRepository.AddUserRoleAsync(userRole);
        }

        public async Task DeleteUserRoleAsync(Guid userId, Guid roleId)
        {
            await _userRoleRepository.DeleteUserRoleAsync(userId, roleId);
        }

        public async Task<IEnumerable<PendingUserRoleDto>> GetPendingUserRolesAsync()
        {
            return await _userRoleRepository.GetPendingUserRolesAsync();
        }

        public async Task<UserRole?> ApproveUserRoleAsync(ApproveRoleDto dto)
        {
            return await _userRoleRepository.ApproveUserRoleAsync(dto);
        }
    }
}
