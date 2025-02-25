using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class LedgerRepository : ILedgerRepository
    {
        private readonly ApplicationDbContext _context;

        public LedgerRepository(ApplicationDbContext context)
        {
            _context = context;
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
            var transactionsList = transactions.Select(t => new FinancialReportCustomDTO
            {
                TransactionId = t.TransactionId,
                TrDate = t.TrDate,
                VrNo = t.VrNo,
                TransactionType = t.TransactionType,
                IncomeAmount = t.IncomeAmount,
                ExpenseAmount = t.ExpenseAmount,
                Description = t.Description,
                HeadId = (customizationOption == FinancialReportCustomizationOption.IdsOnly || customizationOption == FinancialReportCustomizationOption.Both) ? t.HeadId : (int?)null,
                HeadName = (customizationOption == FinancialReportCustomizationOption.NamesOnly || customizationOption == FinancialReportCustomizationOption.Both) ? t.HeadName : null,
                FamilyId = (customizationOption == FinancialReportCustomizationOption.IdsOnly || customizationOption == FinancialReportCustomizationOption.Both) ? t.FamilyId : (int?)null,
                FamilyName = (customizationOption == FinancialReportCustomizationOption.NamesOnly || customizationOption == FinancialReportCustomizationOption.Both) ? t.FamilyName : null,
                BankId = (customizationOption == FinancialReportCustomizationOption.IdsOnly || customizationOption == FinancialReportCustomizationOption.Both) ? t.BankId : (int?)null,
                BankName = (customizationOption == FinancialReportCustomizationOption.NamesOnly || customizationOption == FinancialReportCustomizationOption.Both) ? t.BankName : null,
                ParishId = (customizationOption == FinancialReportCustomizationOption.IdsOnly || customizationOption == FinancialReportCustomizationOption.Both) ? t.ParishId : (int?)null
               // ParishName = (customizationOption == FinancialReportCustomizationOption.NamesOnly || customizationOption == FinancialReportCustomizationOption.Both) ? t.ParishName : null
            }).ToList();

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
                Transactions = includeTransactions ? transactionsList : new List<FinancialReportCustomDTO>()
            };

            return new List<LedgerReportDTO> { report };
        }
    }
}
