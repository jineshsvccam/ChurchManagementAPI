using ChurchData;

public interface IUserService
{
    Task<User> GetUserByIdAsync(Guid userId);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> AddUserAsync(User user, string password, List<Guid> roleIds); // Updated to support password & roles
    Task<User> UpdateUserAsync(User user);
    Task DeleteUserAsync(Guid userId);
}
