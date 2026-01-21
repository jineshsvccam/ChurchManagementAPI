using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories.Settings
{
    public class ContributionSettingsRepository : IContributionSettingsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContributionSettingsRepository(ApplicationDbContext context,
                                IHttpContextAccessor httpContextAccessor,
                                LogsHelper logsHelper)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logsHelper = logsHelper;
        }

        public async Task<IEnumerable<ContributionSettings>> GetAllAsync(int? parishId)
        {
            var query = _context.ContributionSettings.AsNoTracking().AsQueryable();

            if (parishId.HasValue)
            {
                query = query.Where(f => f.ParishId == parishId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<ContributionSettings?> GetByIdAsync(int settingId)
        {
            return await _context.ContributionSettings.FindAsync(settingId);
        }

        public async Task<ContributionSettings> AddAsync(ContributionSettings contributionSettings)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, contributionSettings.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            contributionSettings.ValidFrom = DateTime.SpecifyKind(contributionSettings.ValidFrom, DateTimeKind.Utc);
            await _context.ContributionSettings.AddAsync(contributionSettings);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("contribution_settings", contributionSettings.SettingId, "INSERT", userId, null, Extensions.Serialize(contributionSettings));
            return contributionSettings;
        }

        public async Task<ContributionSettings> UpdateAsync(ContributionSettings contributionSettings)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, contributionSettings.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existing = await _context.ContributionSettings.FindAsync(contributionSettings.SettingId);
            if (existing == null)
            {
                throw new KeyNotFoundException("Contribution setting not found");
            }

            var oldValues = existing.Clone();
            contributionSettings.ValidFrom = DateTime.SpecifyKind(contributionSettings.ValidFrom, DateTimeKind.Utc);
            _context.Entry(existing).CurrentValues.SetValues(contributionSettings);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("contribution_settings", contributionSettings.SettingId, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(contributionSettings));
            return contributionSettings;
        }

        public async Task DeleteAsync(int settingId)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var contributionSetting = await _context.ContributionSettings.FindAsync(settingId);
            if (contributionSetting == null)
            {
                throw new KeyNotFoundException("Contribution setting not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, contributionSetting.ParishId);

            _context.ContributionSettings.Remove(contributionSetting);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("contribution_settings", contributionSetting.SettingId, "DELETE", userId, Extensions.Serialize(contributionSetting), null);
        }
    }
}
