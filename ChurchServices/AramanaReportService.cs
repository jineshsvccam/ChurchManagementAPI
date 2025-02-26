using ChurchContracts;
using ChurchDTOs.DTOs.Utils;

namespace ChurchServices
{
    public class AramanaReportService : IAramanaReportService
    {
        private readonly IAramanaReportRepository _aramanaReportRepository;
        public AramanaReportService(IAramanaReportRepository aramanaReportRepository)
        {
            _aramanaReportRepository = aramanaReportRepository;
        }
        public async Task<AramanaReportDTO> GetAramanaReportAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate)
        {
            // 1. Validate required dates if including transactions
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
