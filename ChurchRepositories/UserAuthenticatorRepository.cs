using ChurchContracts.Interfaces.Repositories;
using ChurchData;
using ChurchData.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class UserAuthenticatorRepository : IUserAuthenticatorRepository
    {
        private readonly ApplicationDbContext _context;

        public UserAuthenticatorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserAuthenticator?> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.UserAuthenticators
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsActive);
        }

        public async Task<UserAuthenticator?> GetByIdAsync(Guid authenticatorId)
        {
            return await _context.UserAuthenticators.FindAsync(authenticatorId);
        }

        public async Task<UserAuthenticator> AddAsync(UserAuthenticator authenticator)
        {
            await _context.UserAuthenticators.AddAsync(authenticator);
            await _context.SaveChangesAsync();
            return authenticator;
        }

        public async Task<UserAuthenticator> UpdateAsync(UserAuthenticator authenticator)
        {
            _context.UserAuthenticators.Update(authenticator);
            await _context.SaveChangesAsync();
            return authenticator;
        }

        public async Task RevokeAllByUserIdAsync(Guid userId)
        {
            var authenticators = await _context.UserAuthenticators
                .Where(a => a.UserId == userId && a.IsActive)
                .ToListAsync();

            foreach (var authenticator in authenticators)
            {
                authenticator.IsActive = false;
                authenticator.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid authenticatorId)
        {
            var authenticator = await _context.UserAuthenticators.FindAsync(authenticatorId);
            if (authenticator != null)
            {
                _context.UserAuthenticators.Remove(authenticator);
                await _context.SaveChangesAsync();
            }
        }
    }
}
