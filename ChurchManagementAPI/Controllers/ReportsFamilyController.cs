using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    public class ReportsFamilyController : FamilyMemberAuthorizedController
    {
        private readonly IFamilyReportService _familyReportService;
        private readonly IKudishikaReportService _kudishikaReportService;
        public ReportsFamilyController(IFamilyReportService familyReportService,
            IKudishikaReportService kudishikaReportService,
             IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ReportsFamilyController> logger)
        {
            _familyReportService = familyReportService;
            _kudishikaReportService = kudishikaReportService;
        }

        [HttpGet("familyReport")]
        public async Task<ActionResult<FamilyReportDTO>> GetFamilyReport(
         [FromQuery] int parishId,
         [FromQuery] int familyNumber)
        {
            var familyReport = await _familyReportService.GetFamilyReportAsync(parishId, familyNumber);
            return Ok(familyReport);
        }

        [HttpGet("kudishikaReport")]
        public async Task<ActionResult<FamilyReportDTO>> GetKudishikaReport(
          [FromQuery] int parishId,
          [FromQuery] int familyNumber,
          [FromQuery] DateTime startDate,
          [FromQuery] DateTime endDate,
          [FromQuery] bool istransactionrequired = true)
        {
            var familyReport = await _kudishikaReportService.GetKudishikaReportAsync(parishId, familyNumber, startDate, endDate, istransactionrequired);
            return Ok(familyReport);
        }
    }
}
