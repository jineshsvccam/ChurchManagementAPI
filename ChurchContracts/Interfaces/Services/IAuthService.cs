using ChurchData;
using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IAuthService
    {
        Task<object> AuthenticateUserAsync(string username, string password, string ipAddress, string userAgent);
        Task<AuthResultDto> VerifyTwoFactorAsync(string tempToken, string code, string ipAddress, string userAgent);
        Task<EnableAuthenticatorResponseDto> SetupTwoFactorAsync(Guid userId);
        Task<EnableAuthenticatorResponseDto> VerifySetupTwoFactorAsync(Guid userId, string code);
        Task<User?> RegisterUserAsync(RegisterDto model);
        Task<bool> DisableTwoFactorAsync(Guid userId, string code);
        Task<TwoFactorStatusDto> GetTwoFactorStatusAsync(Guid userId);
    }
}
