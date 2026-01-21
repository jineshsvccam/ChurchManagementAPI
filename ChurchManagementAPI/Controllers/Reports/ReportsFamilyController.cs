using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchManagementAPI.Controllers.Reports
{
    public class ReportsFamilyController : FamilyMemberAuthorizedController
    {
        private readonly IFamilyReportService _familyReportService;
        private readonly IKudishikaReportService _kudishikaReportService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportsFamilyController> _logger;

        public ReportsFamilyController(IFamilyReportService familyReportService,
            IKudishikaReportService kudishikaReportService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ReportsFamilyController> logger)
        {
            _familyReportService = familyReportService;
            _kudishikaReportService = kudishikaReportService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _logger = logger;
        }

        [HttpGet("familyReport")]
        public async Task<ActionResult<FamilyReportDTO>> GetFamilyReport(
         [FromQuery] int parishId,
         [FromQuery] int familyNumber)
        {
            // Get current user's role, parish, and family information
            var (userRole, userParishId, userFamilyId) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);

            // Verify parish authorization
            if (userParishId != parishId)
            {
                _logger.LogWarning("User attempted to access reports from another parish. UserParishId: {UserParishId}, RequestedParishId: {ParishId}", 
                    userParishId, parishId);
                return Forbid();
            }

            // If the user is a FamilyMember role, ensure they can only access their own family's report
            if (userRole == "FamilyMember")
            {
                // Get the family by familyNumber to extract FamilyId for comparison
                var requestedFamily = await _context.Families
                    .FirstOrDefaultAsync(f => f.ParishId == parishId && f.FamilyNumber == familyNumber);

                if (requestedFamily == null)
                {
                    return NotFound("Family not found.");
                }

                if (requestedFamily.FamilyId != userFamilyId)
                {
                    _logger.LogWarning("User {UserId} with FamilyId {UserFamilyId} attempted to access FamilyId {RequestedFamilyId}",
                        UserHelper.GetCurrentUserIdGuid(_httpContextAccessor), userFamilyId, requestedFamily.FamilyId);
                    return Forbid();
                }
            }

            var familyReport = await _familyReportService.GetFamilyReportAsync(parishId, familyNumber);
            return Ok(familyReport);
        }

        [HttpGet("kudishikaReport")]
        public async Task<ActionResult<KudishikalReportDTO>> GetKudishikaReport(
          [FromQuery] int parishId,
          [FromQuery] int familyNumber,
          [FromQuery] DateTime startDate,
          [FromQuery] DateTime endDate,
          [FromQuery] bool istransactionrequired = true)
        {
            // Get current user's role, parish, and family information
            var (userRole, userParishId, userFamilyId) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);

            // Verify parish authorization
            if (userParishId != parishId)
            {
                _logger.LogWarning("User attempted to access reports from another parish. UserParishId: {UserParishId}, RequestedParishId: {ParishId}", 
                    userParishId, parishId);
                return Forbid();
            }

            // If the user is a FamilyMember role, ensure they can only access their own family's report
            if (userRole == "FamilyMember")
            {
                // Get the family by familyNumber to extract FamilyId for comparison
                var requestedFamily = await _context.Families
                    .FirstOrDefaultAsync(f => f.ParishId == parishId && f.FamilyNumber == familyNumber);

                if (requestedFamily == null)
                {
                    return NotFound("Family not found.");
                }

                if (requestedFamily.FamilyId != userFamilyId)
                {
                    _logger.LogWarning("User {UserId} with FamilyId {UserFamilyId} attempted to access FamilyId {RequestedFamilyId}",
                        UserHelper.GetCurrentUserIdGuid(_httpContextAccessor), userFamilyId, requestedFamily.FamilyId);
                    return Forbid();
                }
            }

            var familyReport = await _kudishikaReportService.GetKudishikaReportAsync(parishId, familyNumber, startDate, endDate, istransactionrequired);
            return Ok(familyReport);
        }
    }
}
