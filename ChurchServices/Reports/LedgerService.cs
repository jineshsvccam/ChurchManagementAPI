using ChurchContracts;
using ChurchDTOs.DTOs.Utils;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Reports
{
    public class LedgerService : ILedgerService
    {
        private readonly ILedgerRepository _ledgerRepository;
        private readonly ILogger<LedgerService> _logger;

        public LedgerService(ILedgerRepository ledgerRepository, ILogger<LedgerService> logger)
        {
            _ledgerRepository = ledgerRepository;
            _logger = logger;
        }

        public async Task<LedgerReportDTO> GetLedgerAsync(
        int parishId, DateTime? startDate, DateTime? endDate, bool includeTransactions = false, FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            _logger.LogInformation("Generating ledger report for ParishId: {ParishId}, StartDate: {StartDate}, EndDate: {EndDate}, IncludeTransactions: {IncludeTransactions}", 
                parishId, startDate, endDate, includeTransactions);

            // Validation: StartDate and EndDate are required if includeTransactions is true
            if (includeTransactions)
            {
                if (!startDate.HasValue || !endDate.HasValue)
                {
                    throw new ArgumentException("Start date and End date are required when including transactions.");
                }

                // Validation: Date range should not exceed 365 days
                if ((endDate.Value - startDate.Value).TotalDays > 365)
                {
                    throw new ArgumentException("The date range cannot exceed 365 days.");
                }
            }

            return await _ledgerRepository.GetLedgerAsync(parishId, startDate, endDate, includeTransactions, customizationOption);
        }
    }
}
