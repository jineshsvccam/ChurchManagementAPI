using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchRepositories
{
    public class FamilyRepository : IFamilyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FamilyRepository> _logger;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FamilyRepository(ApplicationDbContext context,
            ILogger<FamilyRepository> logger, LogsHelper logsHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _logsHelper = logsHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Family>> GetFamiliesAsync(int? parishId, int? unitId, int? familyId)
        {
            _logger.LogInformation("Fetching families with ParishId: {ParishId}, UnitId: {UnitId}, FamilyId: {FamilyId}", parishId, unitId, familyId);

            var query = _context.Families.AsQueryable();

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

            var result = await query.ToListAsync();
            _logger.LogInformation("Fetched {Count} families", result.Count);
            return result;
        }

        public async Task<Family?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching family with Id: {Id}", id);
            var family = await _context.Families.FindAsync(id);

            if (family == null)
            {
                _logger.LogWarning("Family with Id: {Id} not found", id);
            }

            return family;
        }

        public async Task<Family> AddAsync(Family family)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            _logger.LogInformation("Adding new family: {@Family}", family);
            await _context.Families.AddAsync(family);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Family added successfully with Id: {Id}", family.FamilyId);

            await _logsHelper.LogChangeAsync("families", family.FamilyId, "INSERT", userId, null, SerializeFamilies(family));
            return family;
        }

        public async Task<Family> UpdateAsync(Family family)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            _logger.LogInformation("Updating family with Id: {Id}", family.FamilyId);
            var existingFamily = await _context.Families.FindAsync(family.FamilyId);

            if (existingFamily == null)
            {
                _logger.LogWarning("Family with Id: {Id} not found", family.FamilyId);
                throw new KeyNotFoundException("Family not found");
            }
            var oldValues = CloneFamily(existingFamily);

            _context.Entry(existingFamily).CurrentValues.SetValues(family);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Family updated successfully with Id: {Id}", family.FamilyId);
            await _logsHelper.LogChangeAsync("families", family.FamilyId, "UPDATE", userId, SerializeFamilies(oldValues), SerializeFamilies(family));
            return family;
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            _logger.LogInformation("Deleting family with Id: {Id}", id);
            var family = await _context.Families.FindAsync(id);

            if (family == null)
            {
                _logger.LogWarning("Delete failed: Family with Id: {Id} not found", id);
                throw new KeyNotFoundException("Family not found");
            }

            _context.Families.Remove(family);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Family deleted successfully with Id: {Id}", id);
            await _logsHelper.LogChangeAsync("families", family.FamilyId, "DELETE", userId, SerializeFamilies(family), null);
        }

        private string SerializeFamilies(Family family)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(family);
        }
        private Family CloneFamily(Family source)
        {
            return new Family
            {
                FamilyId = source.FamilyId,
                FamilyName = source.FamilyName,
                ParishId = source.ParishId,
                UnitId = source.UnitId,
                Address = source.Address,
                FamilyNumber = source.FamilyNumber,
                HeadName = source.HeadName
            };
        }
    }
}
