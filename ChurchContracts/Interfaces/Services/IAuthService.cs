using ChurchData;
using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IAuthService
    {
        Task<object> AuthenticateUserAsync(string username, string password, string ipAddress, string userAgent);
        Task<User?> RegisterUserAsync(RegisterDto model);
    }
}
