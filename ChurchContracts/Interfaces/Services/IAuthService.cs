using ChurchData;
using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IAuthService
    {
        Task<User?> RegisterUserAsync(RegisterDto model);
    }
}
