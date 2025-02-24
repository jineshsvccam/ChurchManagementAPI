using ChurchData;
using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IContributionSettingsService
    {
        Task<IEnumerable<ContributionSettingsDto>> GetAllAsync(int? parishId);
        Task<ContributionSettingsDto?> GetByIdAsync(int settingId);
        Task<ContributionSettingsDto> AddAsync(ContributionSettingsDto contributionSettingsDto);
        Task<ContributionSettingsDto> UpdateAsync(int settingId, ContributionSettingsDto contributionSettingsDto);
        Task DeleteAsync(int settingId);
        Task<IEnumerable<ContributionSettingsDto>> AddOrUpdateAsync(IEnumerable<ContributionSettingsDto> requests);
    }

}
