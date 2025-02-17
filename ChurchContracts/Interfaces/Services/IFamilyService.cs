using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IFamilyService
    {
        Task<IEnumerable<FamilyDto>> GetFamiliesAsync(int? parishId, int? unitId, int? familyId);
        Task<FamilyDto?> GetByIdAsync(int id);
        Task<IEnumerable<FamilyDto>> AddOrUpdateAsync(IEnumerable<FamilyDto> requests);
        Task<FamilyDto> UpdateAsync(FamilyDto familyDto);
        Task DeleteAsync(int id);
        Task<FamilyDto> AddAsync(FamilyDto familyDto);
    }
}
