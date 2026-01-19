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
    }
}
