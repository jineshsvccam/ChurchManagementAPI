using ChurchData;
using ChurchData.Entities;
using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts.Interfaces.Services
{
    public interface ITwoFactorAuthenticationService
    {
        Task<EnableAuthenticatorResponseDto> EnableAuthenticatorAsync(Guid userId);
        Task<bool> VerifyAuthenticatorAsync(Guid userId, string code);
        Task DisableAuthenticatorAsync(Guid userId);
        Task<TwoFactorRequiredResponseDto> CreateTwoFactorSessionAsync(Guid userId, string ipAddress, string userAgent);
        Task<User?> VerifyTwoFactorLoginAsync(string tempToken, string code);
        Task<bool> VerifyRecoveryCodeAsync(Guid userId, string code);
        Task<List<string>> RegenerateRecoveryCodesAsync(Guid userId);
        Task<IEnumerable<RecoveryCodeDto>> GetRecoveryCodesAsync(Guid userId);
        Task IncrementSessionAttemptsAsync(string tempToken);
    }
}
