using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IAuthService
    {
        Task<string> RegisterUserAsync(RegisterDto model);
    }
}
