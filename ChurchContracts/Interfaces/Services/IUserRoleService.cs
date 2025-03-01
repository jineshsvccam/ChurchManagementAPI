using ChurchData;

namespace ChurchContracts
{
    public interface IUserRoleService
    {
        Task<UserRole> GetUserRoleByIdAsync(Guid userId, Guid roleId); // Updated to use composite key
        Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(Guid userId);
        Task<UserRole> AddUserRoleAsync(UserRole userRole);
        Task DeleteUserRoleAsync(Guid userId, Guid roleId); // Updated to use composite key
    }
}
