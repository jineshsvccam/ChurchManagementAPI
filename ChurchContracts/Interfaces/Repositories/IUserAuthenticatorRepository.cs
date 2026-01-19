using ChurchData.Entities;

namespace ChurchContracts.Interfaces.Repositories
{
    public interface IUserAuthenticatorRepository
    {
        Task<UserAuthenticator?> GetActiveByUserIdAsync(Guid userId);
        Task<UserAuthenticator?> GetByIdAsync(Guid authenticatorId);
        Task<UserAuthenticator> AddAsync(UserAuthenticator authenticator);
        Task<UserAuthenticator> UpdateAsync(UserAuthenticator authenticator);
        Task RevokeAllByUserIdAsync(Guid userId);
        Task DeleteAsync(Guid authenticatorId);
    }
}
