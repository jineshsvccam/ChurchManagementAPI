using ChurchContracts;
using ChurchDTOs.DTOs.Utils;

namespace ChurchServices
{
    public class LedgerService : ILedgerService
    {
        private readonly ILedgerRepository _ledgerRepository;

        public LedgerService(ILedgerRepository ledgerRepository)
        {
            _ledgerRepository = ledgerRepository;
        }

        public async Task<IEnumerable<LedgerReportDTO>> GetLedgerAsync(
        int parishId, DateTime? startDate, DateTime? endDate, bool includeTransactions = false, FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
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
