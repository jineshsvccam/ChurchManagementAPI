using ChurchContracts;
using ChurchDTOs.DTOs.Utils;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Reports
{
    public class PivotReportService : IPivotReportService, ISingleHeadFiscalReportService, IMonthlyFiscalReportService
    {
        private readonly IPivotReportRepository _pivotReportRepository;
        private readonly ISingleHeadFiscalReportRepository _singleHeadFiscalReportRepository;
        private readonly IMonthlyFiscalReportRepository _monthlyFiscalReportRepository;
        private readonly ILogger<PivotReportService> _logger;

        public PivotReportService(
            IPivotReportRepository pivotReportRepository,
            ISingleHeadFiscalReportRepository singleHeadFiscalReportRepository,
            IMonthlyFiscalReportRepository monthlyFiscalReportRepository,
            ILogger<PivotReportService> logger)
        {
            _pivotReportRepository = pivotReportRepository;
            _singleHeadFiscalReportRepository = singleHeadFiscalReportRepository;
            _monthlyFiscalReportRepository = monthlyFiscalReportRepository;
            _logger = logger;
        }

        public async Task<PivotReportResult> GetPivotReportAsync(int parishId, int year, string type, int[]? headIds = null, int? headCount = null)
        {
            _logger.LogInformation("Generating pivot report for ParishId: {ParishId}, Year: {Year}, Type: {Type}", 
                parishId, year, type);
            return await _pivotReportRepository.GetPivotReportAsync(parishId, year, type, headIds, headCount);
        }

        public async Task<SingleHeadFiscalReportDto> GetSingleHeadFiscalReportAsync(int parishId, int headId, string type, int startYear, int endYear)
        {
            _logger.LogInformation("Generating single head fiscal report for ParishId: {ParishId}, HeadId: {HeadId}, Type: {Type}, StartYear: {StartYear}, EndYear: {EndYear}", 
                parishId, headId, type, startYear, endYear);
            return await _singleHeadFiscalReportRepository.GetSingleHeadFiscalReportAsync(parishId, headId, type, startYear, endYear);
        }

        public async Task<MonthlyFiscalReportResponse> GetMonthlyFiscalReportAsync(int parishId, int startYear, int endYear)
        {
            _logger.LogInformation("Generating monthly fiscal report for ParishId: {ParishId}, StartYear: {StartYear}, EndYear: {EndYear}", 
                parishId, startYear, endYear);
            return await _monthlyFiscalReportRepository.GetMonthlyFiscalReportAsync(parishId, startYear, endYear);
        }
    }
}
