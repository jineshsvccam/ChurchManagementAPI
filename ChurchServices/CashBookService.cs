using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchDTOs.DTOs.Utils;

namespace ChurchServices
{
    public class CashBookService : ICashBookService
    {
        private readonly ICashBookRepository _cashBookRepository;
        public CashBookService(ICashBookRepository cashBookRepository)
        {
            _cashBookRepository = cashBookRepository;
        }
        public async Task<CashBookReportDTO> GetCashBookAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            string bankName = "All",
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


            // 3. Convert dates to UTC if they're Unspecified
            DateTime? startUtc = null;
            DateTime? endUtc = null;

            if (startDate.HasValue)
            {
                startUtc = (startDate.Value.Kind == DateTimeKind.Unspecified)
                    ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                    : startDate.Value;
            }

            if (endDate.HasValue)
            {
                endUtc = (endDate.Value.Kind == DateTimeKind.Unspecified)
                    ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
                    : endDate.Value;
            }

            // 4. Call the repository method with validated/converted values
            return await _cashBookRepository.GetCashBookAsync(
                parishId,
                startUtc,
                endUtc,
                bankName,
                customizationOption
            );
        }

    }
}
