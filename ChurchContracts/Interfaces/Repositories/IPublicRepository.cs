using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts.Interfaces.Repositories
{
    public interface IPublicRepository
    {
        Task<ParishesAllDto> GetAllParishesAsync();
        Task<IEnumerable<TransactionHeadBasicDto>> GetTransactionHeadsByParishIdAsync(int parishId);
    }
}
