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
        public async Task<LedgerReportDTO> GetLedgerAsync(
     int parishId,
     DateTime? startDate,
     DateTime? endDate,
     bool includeTransactions = false,
     FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            // Prepare base query filtered by ParishId
            var query = _context.FinancialReportsView
                .Where(r => r.ParishId == parishId)
                .AsQueryable();

            // Apply Start Date filter if provided
            if (startDate.HasValue)
            {
                var start = startDate.Value.Kind == DateTimeKind.Unspecified
                            ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                            : startDate.Value;
                query = query.Where(r => r.TrDate >= start);
            }

            // Apply End Date filter if provided
            if (endDate.HasValue)
            {
                var end = endDate.Value.Kind == DateTimeKind.Unspecified
                          ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
                          : endDate.Value;
                query = query.Where(r => r.TrDate <= end);
            }

            query = query.Where(h => h.HeadName != "Contra");

            // Fetch the filtered transaction records from the view
            var transactions = await query.ToListAsync();

            // Conditionally map transactions to custom DTOs using AutoMapper
            var mappedTransactions = includeTransactions
                ? _mapper.Map<List<FinancialReportCustomDTO>>(transactions, opts =>
                    opts.Items["CustomizationOption"] = customizationOption)
                : new List<FinancialReportCustomDTO>();

            // Calculate overall totals
            var totalIncome = transactions.Sum(t => t.IncomeAmount);
            var totalExpense = transactions.Sum(t => t.ExpenseAmount);

            // Group transactions by Head and compute summary for each
            var groupedTransactions = transactions
                .GroupBy(t => new { t.HeadId, t.HeadName })
                .Select(g =>
                {
                    var income = g.Sum(t => t.IncomeAmount);
                    var expense = g.Sum(t => t.ExpenseAmount);

                    return new HeadDTO
                    {
                        HeadId = (customizationOption == FinancialReportCustomizationOption.IdsOnly || customizationOption == FinancialReportCustomizationOption.Both)
                            ? g.Key.HeadId
                            : (int?)null,

                        HeadName = (customizationOption == FinancialReportCustomizationOption.NamesOnly || customizationOption == FinancialReportCustomizationOption.Both)
                            ? g.Key.HeadName
                            : null,

                        IncomeAmount = income,
                        ExpenseAmount = expense,
                        Balance = income - expense,

                        // Calculate income and expense percentages based on overall totals
                        IncomePercentage = totalIncome > 0 ? Math.Round((income / totalIncome) * 100, 2) : 0,
                        ExpensePercentage = totalExpense > 0 ? Math.Round((expense / totalExpense) * 100, 2) : 0,

                        // Attach transactions for each head if requested
                        Transactions = includeTransactions
                            ? mappedTransactions.Where(mt => mt.HeadId == g.Key.HeadId).ToList()
                            : new List<FinancialReportCustomDTO>()
                    };
                })
                .ToList();

            // Construct final report DTO
            var report = new LedgerReportDTO
            {
                ParishId = parishId,
                StartDate = startDate,
                EndDate = endDate,
                ReportName = "Ledger",
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = totalIncome - totalExpense,
                Heads = groupedTransactions
                        .OrderByDescending(h => h.IncomePercentage)
                        .ThenByDescending(h => h.ExpensePercentage)
                        .ToList()
            };

            return report;
        }
    }
}
