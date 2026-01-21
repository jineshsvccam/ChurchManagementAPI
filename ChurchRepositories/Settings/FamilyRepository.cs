using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories.Settings
{
    public class FamilyRepository : IFamilyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FamilyRepository(ApplicationDbContext context,
            LogsHelper logsHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logsHelper = logsHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Family>> GetFamiliesAsync(int? parishId, int? unitId, int? familyId)
        {
            var query = _context.Families.AsNoTracking().AsQueryable();

            if (parishId.HasValue)
            {
                query = query.Where(f => f.ParishId == parishId.Value);
            }
            if (unitId.HasValue)
            {
                query = query.Where(f => f.UnitId == unitId.Value);
            }
            if (familyId.HasValue)
            {
                query = query.Where(f => f.FamilyId == familyId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Family?> GetByIdAsync(int id)
        {
            return await _context.Families.FindAsync(id);
        }

        public async Task<Family> AddAsync(Family family)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, family.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            await _context.Families.AddAsync(family);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("families", family.FamilyId, "INSERT", userId, null, Extensions.Serialize(family));
            return family;
        }

        public async Task<Family> UpdateAsync(Family family)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, family.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existingFamily = await _context.Families.FindAsync(family.FamilyId);
            if (existingFamily == null)
            {
                throw new KeyNotFoundException("Family not found");
            }

            var oldValues = existingFamily.Clone();
            _context.Entry(existingFamily).CurrentValues.SetValues(family);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("families", family.FamilyId, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(family));
            return family;
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var family = await _context.Families.FindAsync(id);
            if (family == null)
            {
                throw new KeyNotFoundException("Family not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, family.ParishId);

            _context.Families.Remove(family);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("families", family.FamilyId, "DELETE", userId, Extensions.Serialize(family), null);
        }
    }
}
