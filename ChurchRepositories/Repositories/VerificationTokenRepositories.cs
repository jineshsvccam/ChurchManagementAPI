using ChurchContracts.Interfaces.Repositories;
using ChurchData;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories.Repositories
{
    public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public EmailVerificationTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EmailVerificationToken> AddAsync(EmailVerificationToken token)
        {
            _context.EmailVerificationTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<EmailVerificationToken?> GetByTokenAsync(string token)
        {
            return await _context.EmailVerificationTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<EmailVerificationToken?> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.EmailVerificationTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task UpdateAsync(EmailVerificationToken token)
        {
            _context.EmailVerificationTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid tokenId)
        {
            var token = await _context.EmailVerificationTokens.FindAsync(tokenId);
            if (token != null)
            {
                _context.EmailVerificationTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllByUserIdAsync(Guid userId)
        {
            var tokens = await _context.EmailVerificationTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (tokens.Any())
            {
                _context.EmailVerificationTokens.RemoveRange(tokens);
                await _context.SaveChangesAsync();
            }
        }
    }

    public class PhoneVerificationTokenRepository : IPhoneVerificationTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public PhoneVerificationTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PhoneVerificationToken> AddAsync(PhoneVerificationToken token)
        {
            _context.PhoneVerificationTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<PhoneVerificationToken?> GetByPhoneAndUserIdAsync(string phoneNumber, Guid userId)
        {
            return await _context.PhoneVerificationTokens
                .FirstOrDefaultAsync(t => t.PhoneNumber == phoneNumber && t.UserId == userId && 
                                          !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<PhoneVerificationToken?> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.PhoneVerificationTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task UpdateAsync(PhoneVerificationToken token)
        {
            _context.PhoneVerificationTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid tokenId)
        {
            var token = await _context.PhoneVerificationTokens.FindAsync(tokenId);
            if (token != null)
            {
                _context.PhoneVerificationTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllByUserIdAsync(Guid userId)
        {
            var tokens = await _context.PhoneVerificationTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (tokens.Any())
            {
                _context.PhoneVerificationTokens.RemoveRange(tokens);
                await _context.SaveChangesAsync();
            }
        }
    }

    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public PasswordResetTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PasswordResetToken> AddAsync(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<PasswordResetToken?> GetByTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<PasswordResetToken?> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task UpdateAsync(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid tokenId)
        {
            var token = await _context.PasswordResetTokens.FindAsync(tokenId);
            if (token != null)
            {
                _context.PasswordResetTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllByUserIdAsync(Guid userId)
        {
            var tokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (tokens.Any())
            {
                _context.PasswordResetTokens.RemoveRange(tokens);
                await _context.SaveChangesAsync();
            }
        }
    }
}
