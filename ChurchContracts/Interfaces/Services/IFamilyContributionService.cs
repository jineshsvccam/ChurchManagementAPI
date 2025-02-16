using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IFamilyContributionService
    {
        Task<IEnumerable<FamilyContributionDto>> GetAllAsync(int? parishId);
        Task<FamilyContributionDto?> GetByIdAsync(int id);
        Task<FamilyContributionDto> AddAsync(FamilyContributionDto familyContributionDto);
        Task<FamilyContributionDto> UpdateAsync(FamilyContributionDto familyContributionDto);
        Task DeleteAsync(int id);
    }
}
