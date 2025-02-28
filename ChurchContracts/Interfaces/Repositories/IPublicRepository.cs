using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts.Interfaces.Repositories
{
    public interface IPublicRepository
    {
        Task<ParishesAllDto> GetAllParishesAsync();
    }
}
