using ChurchContracts.Utils;

namespace ChurchContracts
{
    public interface ITransactionService
    {
        Task<PagedResult<TransactionDto>> GetTransactionsAsync(int? parishId, int? familyId, int? transactionId, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize);
        Task<TransactionDto?> GetByIdAsync(int id);
        Task<IEnumerable<TransactionDto>> AddOrUpdateAsync(IEnumerable<TransactionDto> requests);
        Task<TransactionDto> UpdateAsync(TransactionDto transaction);
        Task DeleteAsync(int id);
        Task<TransactionDto> AddAsync(TransactionDto transaction);
    }
}
