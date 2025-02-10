using ChurchData;

public interface IUserService
{
    Task<User> GetUserByIdAsync(int userId);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> AddUserAsync(User user, string password, List<int> roleIds); // Updated to support password & roles
    Task<User> UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId);
}
