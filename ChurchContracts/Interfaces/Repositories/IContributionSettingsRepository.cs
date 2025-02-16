using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface IContributionSettingsRepository
    {
        Task<IEnumerable<ContributionSettings>> GetAllAsync();
        Task<ContributionSettings?> GetByIdAsync(int settingId);
        Task<ContributionSettings> AddAsync(ContributionSettings contributionSettings);
        Task<ContributionSettings> UpdateAsync(ContributionSettings contributionSettings);
        Task DeleteAsync(int settingId);
    }

}
