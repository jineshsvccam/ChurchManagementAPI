using ChurchContracts;
using ChurchDTOs.DTOs.Utils;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Reports
{
    public class AramanaReportService : IAramanaReportService
    {
        private readonly IAramanaReportRepository _aramanaReportRepository;
        private readonly ILogger<AramanaReportService> _logger;

        public AramanaReportService(IAramanaReportRepository aramanaReportRepository, ILogger<AramanaReportService> logger)
        {
            _aramanaReportRepository = aramanaReportRepository;
            _logger = logger;
        }

        public async Task<AramanaReportDTO> GetAramanaReportAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate)
        {
            _logger.LogInformation("Generating aramana report for ParishId: {ParishId}, StartDate: {StartDate}, EndDate: {EndDate}",
                parishId, startDate, endDate);

            // 1. Validate required dates
            if (!startDate.HasValue || !endDate.HasValue)
            {
                throw new ArgumentException("Start date and End date are required when including transactions.");
            }
            // 2. Ensure the date range does not exceed 365 days
            if ((endDate.Value - startDate.Value).TotalDays > 365)
            {
                throw new ArgumentException("The date range cannot exceed 365 days.");
            }
            return await _aramanaReportRepository.GetAramanaReportAsync(parishId, startDate, endDate);
        }
    }
}
