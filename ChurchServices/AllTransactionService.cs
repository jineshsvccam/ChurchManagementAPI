using ChurchContracts;
using ChurchDTOs.DTOs.Utils;

namespace ChurchServices
{
    public class AllTransactionsService : IAllTransactionsService
    {
        private readonly IAllTransactionsRepository _allTransactionsRepository;
        public AllTransactionsService(IAllTransactionsRepository allTransactionsRepository)
        {
            _allTransactionsRepository = allTransactionsRepository;
        }
        public async Task<AllTransactionReportDTO> GetAllTransactionAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
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
            return await _allTransactionsRepository.GetAllTransactionAsync(parishId, startDate, endDate, customizationOption);
        }

        public async Task<FinancialReportSummaryDTO> GetAllTransactionGroupedAsync(
           int parishId,
           DateTime? startDate,
           DateTime? endDate,
           FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
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
            return await _allTransactionsRepository.GetAllTransactionGroupedAsync(parishId, startDate, endDate, customizationOption);
        }

    }
}
