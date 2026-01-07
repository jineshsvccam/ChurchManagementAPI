using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchData.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchRepositories
{
    public class FamilyFileRepository : IFamilyFileRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FamilyFileRepository> _logger;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FamilyFileRepository(
            ApplicationDbContext context,
            ILogger<FamilyFileRepository> logger,
            LogsHelper logsHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _logsHelper = logsHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FamilyFile?> GetByIdAsync(Guid fileId)
        {
            _logger.LogInformation("Fetching family file with FileId: {FileId}", fileId);
            return await _context.FamilyFiles.FirstOrDefaultAsync(f => f.FileId == fileId);
        }

        public async Task<IEnumerable<FamilyFile>> GetByFamilyAsync(int familyId)
        {
            _logger.LogInformation("Fetching files for FamilyId: {FamilyId}", familyId);
            return await _context.FamilyFiles
                .Where(f => f.FamilyId == familyId)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FamilyFile>> GetByMemberAsync(int familyId, int memberId)
        {
            _logger.LogInformation(
                "Fetching files for FamilyId: {FamilyId}, MemberId: {MemberId}",
                familyId, memberId);

            return await _context.FamilyFiles
                .Where(f => f.FamilyId == familyId && f.MemberId == memberId)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FamilyFile>> GetByTypeAsync(
            int familyId,
            int? memberId,
            string fileType)
        {
            _logger.LogInformation(
                "Fetching files for FamilyId: {FamilyId}, MemberId: {MemberId}, FileType: {FileType}",
                familyId, memberId, fileType);

            var query = _context.FamilyFiles
                .Where(f => f.FamilyId == familyId && f.FileType == fileType);

            if (memberId.HasValue)
            {
                query = query.Where(f => f.MemberId == memberId);
            }
            else
            {
                query = query.Where(f => f.MemberId == null);
            }

            return await query
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }

        public async Task AddAsync(FamilyFile familyFile)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);

            _logger.LogInformation("Adding family file {@FamilyFile}", familyFile);
            await _context.FamilyFiles.AddAsync(familyFile);
            await _context.SaveChangesAsync();

            await _logsHelper.LogChangeAsync(
                "family_files",
                1,
                "INSERT",
                userId,
                null,
                Extensions.Serialize(familyFile)
            );
        }

        public async Task UpdateAsync(FamilyFile familyFile)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);

            _logger.LogInformation("Updating family file FileId: {FileId}", familyFile.FileId);
            var existing = await _context.FamilyFiles.FindAsync(familyFile.FileId);

            if (existing == null)
            {
                _logger.LogWarning("Family file not found FileId: {FileId}", familyFile.FileId);
                throw new KeyNotFoundException("Family file not found");
            }

            var oldValues = Extensions.Serialize(existing);
            _context.Entry(existing).CurrentValues.SetValues(familyFile);
            await _context.SaveChangesAsync();

            await _logsHelper.LogChangeAsync(
                "family_files",
                1,
                "UPDATE",
                userId,
                oldValues,
                Extensions.Serialize(familyFile)
            );
        }

        public async Task DeleteAsync(Guid fileId)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);

            _logger.LogInformation("Deleting family file FileId: {FileId}", fileId);
            var file = await _context.FamilyFiles.FindAsync(fileId);

            if (file == null)
            {
                _logger.LogWarning("Delete failed. FileId not found: {FileId}", fileId);
                throw new KeyNotFoundException("Family file not found");
            }

            _context.FamilyFiles.Remove(file);
            await _context.SaveChangesAsync();

            await _logsHelper.LogChangeAsync(
                "family_files",
                1,
                "DELETE",
                userId,
                Extensions.Serialize(file),
                null
            );
        }
    }
}
