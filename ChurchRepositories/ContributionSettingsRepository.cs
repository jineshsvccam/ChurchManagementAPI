
using ChurchContracts;
using ChurchData;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class ContributionSettingsRepository : IContributionSettingsRepository
        {
            private readonly ApplicationDbContext _context;

            public ContributionSettingsRepository(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<ContributionSettings>> GetAllAsync()
            {
                return await _context.ContributionSettings.ToListAsync();
            }

            public async Task<ContributionSettings?> GetByIdAsync(int settingId)
            {
                return await _context.ContributionSettings.FindAsync(settingId);
            }

            public async Task<ContributionSettings> AddAsync(ContributionSettings contributionSettings)
            {
            contributionSettings.ValidFrom = DateTime.SpecifyKind(contributionSettings.ValidFrom, DateTimeKind.Utc);
            await _context.ContributionSettings.AddAsync(contributionSettings);
                await _context.SaveChangesAsync();
                return contributionSettings;
            }

            public async Task<ContributionSettings> UpdateAsync(ContributionSettings contributionSettings)
            {
                var existing = await _context.ContributionSettings.FindAsync(contributionSettings.SettingId);
                if (existing != null)
                {
                contributionSettings.ValidFrom = DateTime.SpecifyKind(contributionSettings.ValidFrom, DateTimeKind.Utc);
                _context.Entry(existing).CurrentValues.SetValues(contributionSettings);
                    await _context.SaveChangesAsync();
                    return contributionSettings;
                }
                else
                {
                    throw new KeyNotFoundException("Contribution setting not found");
                }
            }

            public async Task DeleteAsync(int settingId)
            {
                var contributionSetting = await _context.ContributionSettings.FindAsync(settingId);
                if (contributionSetting != null)
                {
                    _context.ContributionSettings.Remove(contributionSetting);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new KeyNotFoundException("Contribution setting not found");
                }
            }
        }
    }


