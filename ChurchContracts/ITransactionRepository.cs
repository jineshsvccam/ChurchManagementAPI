using ChurchData;

namespace ChurchContracts
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetTransactionsAsync(int? parishId, int? familyId, int? transactionId);
        Task<Transaction?> GetByIdAsync(int id);
        Task<Transaction> AddAsync(Transaction transaction);
        Task<Transaction> UpdateAsync(Transaction transaction);
        Task DeleteAsync(int id);
    }
}
