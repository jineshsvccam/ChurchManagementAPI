using ChurchData.Entities;

namespace ChurchContracts.Interfaces.Repositories
{
    public interface IUser2FARecoveryCodeRepository
    {
        Task<IEnumerable<User2FARecoveryCode>> GetUnusedByUserIdAsync(Guid userId);
        Task<IEnumerable<User2FARecoveryCode>> GetAllByUserIdAsync(Guid userId);
        Task<User2FARecoveryCode?> GetByIdAsync(Guid recoveryCodeId);
        Task AddRangeAsync(IEnumerable<User2FARecoveryCode> recoveryCodes);
        Task<User2FARecoveryCode> UpdateAsync(User2FARecoveryCode recoveryCode);
        Task DeleteAllByUserIdAsync(Guid userId);
    }
}
