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
                         let latestPhoto = _context.FamilyFiles
                             .Where(ff =>
                                 ff.FamilyId == f.FamilyId &&
                                 ff.FileType == "FamilyPhoto" &&
                                 ff.Status == "Approved")
                             .OrderByDescending(ff => ff.UploadedAt) 
                             .Select(ff => new
                             {
                                 ff.FileId
                             })
                             .FirstOrDefault()
     select new FamilyWithPhotoInfo
     {
         Family = f,
         HasFamilyPhoto = latestPhoto != null,
         FamilyPhotoFileId = latestPhoto != null ? latestPhoto.FileId : null
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
