using ChurchContracts;
using ChurchDTOs.DTOs.Utils;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Transactions
{
    public class AllTransactionsService : IAllTransactionsService
    {
        private readonly IAllTransactionsRepository _allTransactionsRepository;
        private readonly ILogger<AllTransactionsService> _logger;

        public AllTransactionsService(IAllTransactionsRepository allTransactionsRepository, ILogger<AllTransactionsService> logger)
        {
            _allTransactionsRepository = allTransactionsRepository;
            _logger = logger;
        }

        public async Task<AllTransactionReportDTO> GetAllTransactionAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            _logger.LogInformation("Generating all transactions report for ParishId: {ParishId}, StartDate: {StartDate}, EndDate: {EndDate}",
                parishId, startDate, endDate);

            if (!startDate.HasValue || !endDate.HasValue)
            {
                throw new ArgumentException("Start date and End date are required when including transactions.");
            }

            if ((endDate.Value - startDate.Value).TotalDays > 365)
            {
                throw new ArgumentException("The date range cannot exceed 365 days.");
            }
            return await _allTransactionsRepository.GetAllTransactionAsync(parishId, startDate, endDate, customizationOption);
        }

        public async Task<FinancialReportSummaryDTO> GetAllTransactionGroupedAsync(
           int parishId,
           DateTime? startDate,
           DateTime? endDate,
           FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            _logger.LogInformation("Generating all transactions grouped report for ParishId: {ParishId}, StartDate: {StartDate}, EndDate: {EndDate}",
                parishId, startDate, endDate);

            if (!startDate.HasValue || !endDate.HasValue)
            {
                throw new ArgumentException("Start date and End date are required when including transactions.");
            }

            if ((endDate.Value - startDate.Value).TotalDays > 365)
            {
                throw new ArgumentException("The date range cannot exceed 365 days.");
            }
            return await _allTransactionsRepository.GetAllTransactionGroupedAsync(parishId, startDate, endDate, customizationOption);
        }
    }
}
