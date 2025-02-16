using ChurchData;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchContracts
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByUsernameAndPasswordAsync(string username, string password);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> AddUserAsync(User user, string password, List<int> roleIds); // Updated to allow role assignment
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
    }
}
