using ChurchContracts.Interfaces.Repositories;
using ChurchData;
using ChurchData.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class User2FARecoveryCodeRepository : IUser2FARecoveryCodeRepository
    {
        private readonly ApplicationDbContext _context;

        public User2FARecoveryCodeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User2FARecoveryCode>> GetUnusedByUserIdAsync(Guid userId)
        {
            return await _context.User2FARecoveryCodes
                .Where(rc => rc.UserId == userId && !rc.IsUsed)
                .ToListAsync();
        }

        public async Task<IEnumerable<User2FARecoveryCode>> GetAllByUserIdAsync(Guid userId)
        {
            return await _context.User2FARecoveryCodes
                .Where(rc => rc.UserId == userId)
                .ToListAsync();
        }

        public async Task<User2FARecoveryCode?> GetByIdAsync(Guid recoveryCodeId)
        {
            return await _context.User2FARecoveryCodes.FindAsync(recoveryCodeId);
        }

        public async Task AddRangeAsync(IEnumerable<User2FARecoveryCode> recoveryCodes)
        {
            await _context.User2FARecoveryCodes.AddRangeAsync(recoveryCodes);
            await _context.SaveChangesAsync();
        }

        public async Task<User2FARecoveryCode> UpdateAsync(User2FARecoveryCode recoveryCode)
        {
            _context.User2FARecoveryCodes.Update(recoveryCode);
            await _context.SaveChangesAsync();
            return recoveryCode;
        }

        public async Task DeleteAllByUserIdAsync(Guid userId)
        {
            var recoveryCodes = await _context.User2FARecoveryCodes
                .Where(rc => rc.UserId == userId)
                .ToListAsync();

            _context.User2FARecoveryCodes.RemoveRange(recoveryCodes);
            await _context.SaveChangesAsync();
        }
    }
}
