using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;
using ChurchRepositories.Queries.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchRepositories.Queries
{
    public class FamilyQueryRepository : IFamilyQueryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FamilyQueryRepository> _logger;

        public FamilyQueryRepository(
            ApplicationDbContext context,
            ILogger<FamilyQueryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<FamilyWithPhotoInfo>> GetFamiliesWithPhotoAsync(
            int? parishId,
            int? unitId,
            int? familyId)
        {
            _logger.LogInformation(
                "Fetching families with photo info. ParishId: {ParishId}, UnitId: {UnitId}, FamilyId: {FamilyId}",
                parishId, unitId, familyId);

            var query =
                from f in _context.Families
                join ff in _context.FamilyFiles
                    .Where(x =>
                        x.FileType == "FamilyPhoto" &&
                        x.IsPrimary &&
                        x.Status == "Approved")
                    on f.FamilyId equals ff.FamilyId into photoGroup
                from photo in photoGroup.DefaultIfEmpty()
                select new FamilyWithPhotoInfo
                {
                    Family = f,
                    HasFamilyPhoto = photo != null,
                    FamilyPhotoFileId = photo != null ? photo.FileId : null
                };

            if (parishId.HasValue)
                query = query.Where(x => x.Family.ParishId == parishId.Value);

            if (unitId.HasValue)
                query = query.Where(x => x.Family.UnitId == unitId.Value);

            if (familyId.HasValue)
                query = query.Where(x => x.Family.FamilyId == familyId.Value);

            return await query.ToListAsync();
        }
    }
}
