using ChurchContracts.Utils;
using ChurchData;

namespace ChurchContracts
{
    public interface ITransactionService
    {
        Task<PagedResult<Transaction>> GetTransactionsAsync(int? parishId, int? familyId, int? transactionId, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize);
        Task<Transaction?> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> AddOrUpdateAsync(IEnumerable<Transaction> requests);
        Task<Transaction> UpdateAsync(Transaction transaction);
        Task DeleteAsync(int id);
        Task<Transaction> AddAsync(Transaction transaction);
    }
}
