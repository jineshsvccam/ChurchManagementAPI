using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IRecurringTransactionService
    {
        Task<IEnumerable<RecurringTransactionDto>> GetAllAsync(int? parishId);
        Task<RecurringTransactionDto?> GetByIdAsync(int id);
        Task<RecurringTransactionDto> AddAsync(RecurringTransactionDto dto);
        Task<RecurringTransactionDto> UpdateAsync(int id, RecurringTransactionDto dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<RecurringTransactionDto>> AddOrUpdateAsync(IEnumerable<RecurringTransactionDto> requests);
    }
}
