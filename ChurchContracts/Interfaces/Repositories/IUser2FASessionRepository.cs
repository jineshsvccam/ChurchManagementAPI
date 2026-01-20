using ChurchData.Entities;

namespace ChurchContracts.Interfaces.Repositories
{
    public interface IUser2FASessionRepository
    {
        Task<User2FASession?> GetByTempTokenAsync(string tempToken);
        Task<User2FASession?> GetByIdAsync(Guid sessionId);
        Task<User2FASession> AddAsync(User2FASession session);
        Task<User2FASession> UpdateAsync(User2FASession session);
        Task DeleteAsync(Guid sessionId);
        Task DeleteExpiredSessionsAsync();
        Task DeleteAllByUserIdAsync(Guid userId);
        
        /// <summary>
        /// Counts active (non-expired) sessions for a user created within the specified time window.
        /// Used for rate limiting 2FA session creation.
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <param name="windowStart">Start of the time window</param>
        /// <returns>Count of active sessions created after windowStart</returns>
        Task<int> CountActiveSessionsAsync(Guid userId, DateTime windowStart);
    }
}
