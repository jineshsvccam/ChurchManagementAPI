using ChurchData;
using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IParishService
    {
        Task<IEnumerable<ParishDto>> GetAllAsync();
        Task<ParishDto?> GetByIdAsync(int id);
        Task<ParishDto> AddAsync(ParishDto parishDto);
        Task<ParishDto> UpdateAsync(ParishDto parishDto);
        Task DeleteAsync(int id);
        Task<ParishDetailsBasicDto> GetParishDetailsAsync(int parishId, bool includeFamilyMembers = false);
    }
}
