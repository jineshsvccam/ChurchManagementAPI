using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IFamilyDueService
    {
        Task<IEnumerable<FamilyDueDto>> GetAllAsync(int? parishId);
        Task<FamilyDueDto?> GetByIdAsync(int id);
        Task<FamilyDueDto> AddAsync(FamilyDueDto dto);
        Task<FamilyDueDto> UpdateAsync(int id, FamilyDueDto dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<FamilyDueDto>> AddOrUpdateAsync(IEnumerable<FamilyDueDto> requests);
    }
}
