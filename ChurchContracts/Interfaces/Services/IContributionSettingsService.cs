using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;
using ChurchData.DTOs;

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
