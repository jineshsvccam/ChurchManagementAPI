using ChurchData;

namespace ChurchContracts
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetTransactionsAsync(int? parishId, int? familyId, int? transactionId);
        Task<Transaction?> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> AddOrUpdateAsync(IEnumerable<Transaction> requests);
        Task<Transaction> UpdateAsync(Transaction transaction);
        Task DeleteAsync(int id);
        Task<Transaction> AddAsync(Transaction transaction);
    }
}
