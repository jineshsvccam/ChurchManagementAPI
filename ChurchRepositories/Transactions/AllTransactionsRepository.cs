using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories.Transactions
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
                ParishId = parishId,
                StartDate = startDate,
                EndDate = endDate,
                ReportName = "All Transactions",
                Transactions = mappedTransactions
            };
        }


        public async Task<FinancialReportSummaryDTO> GetAllTransactionGroupedAsync(
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

            var income = mappedTransactions
                .Where(tx => tx.IncomeAmount > 0 && tx.HeadName != "Contra" && !tx.VrNo.Contains("/"))
                .GroupBy(tx => tx.VrNo)
                .Select(group => new VRGroupedDTO
                {
                    VrNo = group.Key,
                    Date = group.First().TrDate,
                    BillName = group.First().BillName,
                    FamilyNumber = group.First().FamilyNumber,
                    Amount = group.Sum(tx => tx.IncomeAmount),
                    Count = group.Count(),
                    Details = group.ToList()
                }).ToList();

            var expense = mappedTransactions
                .Where(tx => tx.ExpenseAmount > 0 && tx.HeadName != "Contra" && !tx.VrNo.Contains("/"))
                .GroupBy(tx => tx.VrNo)
                .Select(group => new VRGroupedDTO
                {
                    VrNo = group.Key,
                    Date = group.First().TrDate,
                    BillName = group.First().BillName,
                    FamilyNumber = group.First().FamilyNumber,
                    Amount = group.Sum(tx => tx.ExpenseAmount),
                    Count = group.Count(),
                    Details = group.ToList()
                }).ToList();

           var bankTransfers = mappedTransactions
                .Where(tx => tx.HeadName == "Contra")
                .GroupBy(tx => tx.VrNo)
                .Select(group => new VRGroupedDTO
                {
                    VrNo = group.Key,
                    Date = group.First().TrDate,
                    BillName = $"{group.First().BillName}  {group.Last().BillName}",
                    FamilyNumber = group.First().FamilyNumber,
                    Amount = group.Sum(tx => tx.IncomeAmount),
                    Count = group.Count(),
                    Details = group.ToList()
                }).ToList();

                var bulkEntry = mappedTransactions
                    .Where(tx => tx.VrNo.Contains("/"))
                    .GroupBy(tx => tx.VrNo.Split('/')[0])
                    .Select(group => new VRGroupedDTO
                    {
                        VrNo = group.Key,
                        Date = group.First().TrDate,
                        BillName = group.First().HeadName,
                        FamilyNumber = group.First().FamilyNumber,
                        Amount = group.Sum(tx => tx.IncomeAmount),
                        Count = group.Count(),
                        Details = group.ToList()
                    }).ToList();

            return new FinancialReportSummaryDTO
            {
                ParishId = parishId,
                StartDate = startDate,
                EndDate = endDate,
                ReportName = "All Transactions Grouped",

                Income = income,
                Expense = expense,
                BankTransfers = bankTransfers,
                BulkEntry = bulkEntry
            };
        }
    }
}
