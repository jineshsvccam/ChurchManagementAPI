using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchRepositories
{
    public class FamilyContributionRepository : IFamilyContributionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TransactionHeadRepository> _logger;

        public FamilyContributionRepository(ApplicationDbContext context,
                                  IHttpContextAccessor httpContextAccessor,
                                  ILogger<TransactionHeadRepository> logger,
                                   LogsHelper logsHelper)        
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logsHelper = logsHelper;
            _logger = logger;
        }

        public async Task<IEnumerable<FamilyContribution>> GetAllAsync(int? parishId)
        {
            var query = _context.FamilyContributions.AsQueryable();
            if (parishId.HasValue)
                query = query.Where(fc => fc.ParishId == parishId.Value);
            return await query.ToListAsync();
        }

        public async Task<FamilyContribution?> GetByIdAsync(int id)
        {
            return await _context.FamilyContributions.FindAsync(id);
        }

        public async Task<FamilyContribution> AddAsync(FamilyContribution familyContribution)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            familyContribution.TransactionDate = DateTime.SpecifyKind(familyContribution.TransactionDate, DateTimeKind.Utc);
            await _context.FamilyContributions.AddAsync(familyContribution);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("family_contributions", familyContribution.ContributionId, "INSERT", userId, null, Extensions.Serialize(familyContribution));
            return familyContribution;
        }

        public async Task<FamilyContribution> UpdateAsync(FamilyContribution familyContribution)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existingContribution = await _context.FamilyContributions.FindAsync(familyContribution.ContributionId);
            if (existingContribution != null)
            {
                var oldValues = existingContribution.Clone();
                _context.Entry(existingContribution).CurrentValues.SetValues(familyContribution);
                familyContribution.TransactionDate = DateTime.SpecifyKind(familyContribution.TransactionDate, DateTimeKind.Utc);
                await _context.SaveChangesAsync();
                await _logsHelper.LogChangeAsync("family_contributions", familyContribution.ContributionId, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(familyContribution));
                return familyContribution;
            }
            else
            {
                throw new KeyNotFoundException("Family Contribution not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var contribution = await _context.FamilyContributions.FindAsync(id);
            if (contribution != null)
            {
                _context.FamilyContributions.Remove(contribution);
                await _context.SaveChangesAsync();
                await _logsHelper.LogChangeAsync("family_contributions", contribution.ContributionId, "DELETE", userId, null, Extensions.Serialize(contribution));
            }
            else
            {
                throw new KeyNotFoundException("Family Contribution not found");
            }
        }
    }
}
