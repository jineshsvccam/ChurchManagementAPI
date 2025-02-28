using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts.Interfaces.Services
{
    public interface IPublicService
    {
        Task<ParishesAllDto> GetAllParishesAsync();
    }
}
