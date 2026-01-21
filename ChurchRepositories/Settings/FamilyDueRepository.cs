using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories.Settings
{
    public class FamilyDueRepository : IFamilyDueRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FamilyDueRepository(ApplicationDbContext context,
                                 IHttpContextAccessor httpContextAccessor,
                                 LogsHelper logsHelper)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logsHelper = logsHelper;
        }

        public async Task<IEnumerable<FamilyDue>> GetAllAsync(int? parishId)
        {
            var query = _context.FamilyDues.AsNoTracking().AsQueryable();
            if (parishId.HasValue)
            {
                query = query.Where(fd => fd.ParishId == parishId.Value);
            }
            return await query.ToListAsync();
        }

        public async Task<FamilyDue?> GetByIdAsync(int id)
        {
            return await _context.FamilyDues.FindAsync(id);
        }

        public async Task<FamilyDue> AddAsync(FamilyDue familyDue)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, familyDue.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            await _context.FamilyDues.AddAsync(familyDue);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("family_dues", familyDue.DuesId, "INSERT", userId, null, Extensions.Serialize(familyDue));
            return familyDue;
        }

        public async Task<FamilyDue> UpdateAsync(FamilyDue familyDue)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, familyDue.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existingDue = await _context.FamilyDues.FindAsync(familyDue.DuesId);
            if (existingDue == null)
            {
                throw new KeyNotFoundException("Family due not found");
            }

            var oldValues = existingDue.Clone();
            _context.Entry(existingDue).CurrentValues.SetValues(familyDue);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("family_dues", familyDue.DuesId, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(familyDue));
            return familyDue;
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var due = await _context.FamilyDues.FindAsync(id);
            if (due == null)
            {
                throw new KeyNotFoundException("Family due not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, due.ParishId);

            _context.FamilyDues.Remove(due);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("family_dues", due.DuesId, "DELETE", userId, Extensions.Serialize(due), null);
        }
    }
}
