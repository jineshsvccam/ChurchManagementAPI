using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface ITransactionHeadService
    {
        Task<IEnumerable<TransactionHeadDto>> GetTransactionHeadsAsync(int? parishId, int? headId);
        Task<TransactionHeadDto?> GetByIdAsync(int id);
        Task<IEnumerable<TransactionHeadDto>> AddOrUpdateAsync(IEnumerable<TransactionHeadDto> requests);
        Task<TransactionHeadDto> UpdateAsync(TransactionHeadDto transactionHeadDto);
        Task DeleteAsync(int id);
        Task<TransactionHeadDto> AddAsync(TransactionHeadDto transactionHeadDto);
    }
}
