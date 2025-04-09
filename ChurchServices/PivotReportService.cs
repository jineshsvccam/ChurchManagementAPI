using ChurchContracts;
using ChurchDTOs.DTOs.Utils;

namespace ChurchServices
{
    public class PivotReportService : IPivotReportService, ISingleHeadFiscalReportService, IMonthlyFiscalReportService
    {
        private readonly IPivotReportRepository _pivotReportRepository;
        private readonly ISingleHeadFiscalReportRepository _singleHeadFiscalReportRepository;
        private readonly IMonthlyFiscalReportRepository _monthlyFiscalReportRepository;

        public PivotReportService(
            IPivotReportRepository pivotReportRepository,
            ISingleHeadFiscalReportRepository singleHeadFiscalReportRepository,
            IMonthlyFiscalReportRepository monthlyFiscalReportRepository)
        {
            _pivotReportRepository = pivotReportRepository;
            _singleHeadFiscalReportRepository = singleHeadFiscalReportRepository;
            _monthlyFiscalReportRepository = monthlyFiscalReportRepository;
        }

        public async Task<PivotReportResult> GetPivotReportAsync(int parishId, int year, string type, int[]? headIds = null, int? headCount = null)
        {
            return await _pivotReportRepository.GetPivotReportAsync(parishId, year, type, headIds);
        }

        public async Task<SingleHeadFiscalReportDto> GetSingleHeadFiscalReportAsync(int parishId, int headId, string type, int startYear, int endYear)
        {
            return await _singleHeadFiscalReportRepository.GetSingleHeadFiscalReportAsync(parishId, headId, type, startYear, endYear);
        }

        public async Task<MonthlyFiscalReportResponse> GetMonthlyFiscalReportAsync(int parishId, int startYear, int endYear)
        {
            return await _monthlyFiscalReportRepository.GetMonthlyFiscalReportAsync(parishId, startYear, endYear);
        }
    }
}
