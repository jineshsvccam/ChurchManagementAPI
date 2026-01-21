using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories.Settings
{
    public class UnitRepository : IUnitRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UnitRepository(ApplicationDbContext context,
                              IHttpContextAccessor httpContextAccessor,
                              LogsHelper logsHelper)
        {
            _context = context;
            _logsHelper = logsHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Unit>> GetAllAsync(int? parishId)
        {
            var query = _context.Units.AsNoTracking().AsQueryable();
            if (parishId.HasValue)
            {
                query = query.Where(u => u.ParishId == parishId.Value);
            }
            return await query.ToListAsync();
        }

        public async Task<Unit?> GetByIdAsync(int id)
        {
            return await _context.Units.FindAsync(id);
        }

        public async Task<Unit> AddAsync(Unit unit)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, unit.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            await _context.Units.AddAsync(unit);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("units", unit.UnitId, "INSERT", userId, null, Extensions.Serialize(unit));
            return unit;
        }

        public async Task<Unit> UpdateAsync(Unit unit)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, unit.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existingUnit = await _context.Units.FindAsync(unit.UnitId);
            if (existingUnit == null)
            {
                throw new KeyNotFoundException("Unit not found");
            }

            var oldValues = existingUnit.Clone();
            _context.Entry(existingUnit).CurrentValues.SetValues(unit);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("units", unit.UnitId, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(unit));
            return unit;
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var unit = await _context.Units.FindAsync(id);
            if (unit == null)
            {
                throw new KeyNotFoundException("Unit not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, unit.ParishId);

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("units", unit.UnitId, "DELETE", userId, Extensions.Serialize(unit), null);
        }
    }
}
