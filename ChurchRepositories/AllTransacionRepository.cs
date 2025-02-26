using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class AllTransacionsRepository : IAllTransactionsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AllTransacionsRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<AllTransactionReportDTO> GetAllTransactionAsync(
            int parishId,
            DateTime? startDate,
            DateTime? endDate,
            FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            // Normalize dates to UTC if provided.
            DateTime? startUtc = startDate.HasValue
                ? (startDate.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                    : startDate.Value)
                : (DateTime?)null;
            DateTime? endUtc = endDate.HasValue
                ? (endDate.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
                    : endDate.Value)
                : (DateTime?)null;

            var transactions = await _context.FinancialReportsView
                .Where(tx => tx.ParishId == parishId)
                .Where(tx => !startUtc.HasValue || tx.TrDate >= startUtc)
                .Where(tx => !endUtc.HasValue || tx.TrDate <= endUtc)
                .ToListAsync();

            // Use AutoMapper to map transactions into FinancialReportCustomDTO list.
            var mappedTransactions = _mapper.Map<List<FinancialReportCustomDTO>>(transactions, opts =>
                    opts.Items["CustomizationOption"] = customizationOption);
                
            return new AllTransactionReportDTO
            {
                Transactions = mappedTransactions
            };
        }

    }
}
