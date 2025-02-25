using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class LedgerRepository : ILedgerRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LedgerRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LedgerReportDTO>> GetLedgerAsync(
                                 int parishId,
                                 DateTime? startDate,
                                 DateTime? endDate,
                                 bool includeTransactions = false,
                                 FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            var query = _context.FinancialReportsView
                 .Where(r => r.ParishId == parishId)
                 .AsQueryable();

            if (startDate.HasValue)
            {
                var start = startDate.Value.Kind == DateTimeKind.Unspecified
                            ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                            : startDate.Value;
                query = query.Where(r => r.TrDate >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Kind == DateTimeKind.Unspecified
                          ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
                          : endDate.Value;
                query = query.Where(r => r.TrDate <= end);
            }

            var transactions = await query.ToListAsync();

            // Map each transaction into our custom DTO based on the chosen customization option.

            // Use AutoMapper to map transactions into FinancialReportCustomDTO list.
            var mappedTransactions = includeTransactions
                 ? _mapper.Map<List<FinancialReportCustomDTO>>(transactions, opts =>
                    opts.Items["CustomizationOption"] = customizationOption)
                 : new List<FinancialReportCustomDTO>();

            // Group transactions by Head. We group by both HeadId and HeadName but adjust the output based on customization.
            var groupedTransactions = transactions
                 .GroupBy(t => new { t.HeadId, t.HeadName })
                 .Select(g => new HeadDTO
                 {
                     HeadId = (customizationOption == FinancialReportCustomizationOption.IdsOnly || customizationOption == FinancialReportCustomizationOption.Both) ? g.Key.HeadId : (int?)null,
                     HeadName = (customizationOption == FinancialReportCustomizationOption.NamesOnly || customizationOption == FinancialReportCustomizationOption.Both) ? g.Key.HeadName : null,
                     IncomeAmount = g.Sum(t => t.IncomeAmount),
                     ExpenseAmount = g.Sum(t => t.ExpenseAmount),
                     Balance = g.Sum(t => t.IncomeAmount) - g.Sum(t => t.ExpenseAmount)
                 }).ToList();

            var report = new LedgerReportDTO
            {
                Heads = groupedTransactions,
                Transactions = includeTransactions ? mappedTransactions : new List<FinancialReportCustomDTO>()
            };

            return new List<LedgerReportDTO> { report };
        }
    }
}
