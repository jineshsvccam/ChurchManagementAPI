using ChurchContracts;
using ChurchData;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchServices
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
    }
}
