using ChurchData;

namespace ChurchContracts
{
    public interface IUserRoleService
    {
        Task<UserRole> GetUserRoleByIdAsync(int userId, int roleId); // Updated to use composite key
        Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(int userId);
        Task<UserRole> AddUserRoleAsync(UserRole userRole);
        Task DeleteUserRoleAsync(int userId, int roleId); // Updated to use composite key
    }
}
