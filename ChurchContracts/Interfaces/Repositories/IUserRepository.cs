using ChurchData;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchContracts
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByUsernameAndPasswordAsync(string username, string password);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> AddUserAsync(User user, string password, List<Guid> roleIds); // Updated to allow role assignment
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid userId);
    }
}
