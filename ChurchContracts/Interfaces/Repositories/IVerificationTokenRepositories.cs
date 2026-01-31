using ChurchData;

namespace ChurchContracts.Interfaces.Repositories
{
    public interface IEmailVerificationTokenRepository
    {
        Task<EmailVerificationToken> AddAsync(EmailVerificationToken token);
        Task<EmailVerificationToken?> GetByTokenAsync(string token);
        Task<EmailVerificationToken?> GetActiveByUserIdAsync(Guid userId);
        Task UpdateAsync(EmailVerificationToken token);
        Task DeleteAsync(Guid tokenId);
        Task DeleteAllByUserIdAsync(Guid userId);
    }

    public interface IPhoneVerificationTokenRepository
    {
        Task<PhoneVerificationToken> AddAsync(PhoneVerificationToken token);
        Task<PhoneVerificationToken?> GetByPhoneAndUserIdAsync(string phoneNumber, Guid userId);
        Task<PhoneVerificationToken?> GetActiveByUserIdAsync(Guid userId);
        Task UpdateAsync(PhoneVerificationToken token);
        Task DeleteAsync(Guid tokenId);
        Task DeleteAllByUserIdAsync(Guid userId);
    }

    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken> AddAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetByTokenAsync(string token);
        Task<PasswordResetToken?> GetActiveByUserIdAsync(Guid userId);
        Task UpdateAsync(PasswordResetToken token);
        Task DeleteAsync(Guid tokenId);
        Task DeleteAllByUserIdAsync(Guid userId);
    }
}
