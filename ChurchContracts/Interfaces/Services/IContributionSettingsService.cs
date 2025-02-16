using ChurchData;
using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IContributionSettingsService
    {
        Task<IEnumerable<ContributionSettingsDto>> GetAllAsync();
        Task<ContributionSettingsDto?> GetByIdAsync(int settingId);
        Task<ContributionSettings> AddAsync(ContributionSettingsDto contributionSettingsDto);
        Task<ContributionSettings> UpdateAsync(int settingId, ContributionSettingsDto contributionSettingsDto);
        Task DeleteAsync(int settingId);
    }

}
