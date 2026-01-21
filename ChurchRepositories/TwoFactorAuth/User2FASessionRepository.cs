using ChurchContracts.Interfaces.Repositories;
using ChurchData;
using ChurchData.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories.TwoFactorAuth
{
    public class User2FASessionRepository : IUser2FASessionRepository
    {
        private readonly ApplicationDbContext _context;

        public User2FASessionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User2FASession?> GetByTempTokenAsync(string tempToken)
        {
            return await _context.User2FASessions
                .FirstOrDefaultAsync(s => s.TempToken == tempToken);
        }

        public async Task<User2FASession?> GetByIdAsync(Guid sessionId)
        {
            return await _context.User2FASessions.FindAsync(sessionId);
        }

        public async Task<User2FASession> AddAsync(User2FASession session)
        {
            await _context.User2FASessions.AddAsync(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<User2FASession> UpdateAsync(User2FASession session)
        {
            _context.User2FASessions.Update(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task DeleteAsync(Guid sessionId)
        {
            var session = await _context.User2FASessions.FindAsync(sessionId);
            if (session != null)
            {
                _context.User2FASessions.Remove(session);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteExpiredSessionsAsync()
        {
            var expiredSessions = await _context.User2FASessions
                .Where(s => s.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            _context.User2FASessions.RemoveRange(expiredSessions);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllByUserIdAsync(Guid userId)
        {
            var sessions = await _context.User2FASessions
                .Where(s => s.UserId == userId)
                .ToListAsync();

            _context.User2FASessions.RemoveRange(sessions);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountActiveSessionsAsync(Guid userId, DateTime windowStart)
        {
            // Count sessions that are:
            // 1. Belonging to the specified user
            // 2. Created within the rate limit window
            // 3. Not yet expired
            // This query uses indexed columns (user_id, created_at, expires_at)
            return await _context.User2FASessions
                .CountAsync(s => 
                    s.UserId == userId && 
                    s.CreatedAt >= windowStart && 
                    s.ExpiresAt > DateTime.UtcNow);
        }
    }
}
