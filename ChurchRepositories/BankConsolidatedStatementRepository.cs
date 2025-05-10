using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace ChurchRepositories
{
    public class BankConsolidatedStatementRepository : IBankConsolidatedStatementRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BankConsolidatedStatementRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<BankStatementConsolidatedDTO> GetBankStatementAsync(
        int parishId,
        DateTime? startDate,
        DateTime? endDate,
        bool includeTransactions = false,
        FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            var startDateValue = startDate.HasValue
                ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                : DateTime.MinValue;
            var endDateValue = endDate.HasValue
                ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
                : DateTime.MaxValue;

            var query = _context.FinancialReportsView
                .Where(r => r.ParishId == parishId && r.BankName != "JE")
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(r => r.TrDate >= startDateValue);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.TrDate <= endDateValue);
            }

            var transactions = await query.ToListAsync();

            var parishIdParam = new NpgsqlParameter<int>("in_parish_id", parishId);
            var startDateParam = new NpgsqlParameter<DateTime>("in_start_date", startDateValue.Date);
            var endDateParam = new NpgsqlParameter<DateTime>("in_end_date", endDateValue.Date);

            var rawResults = await _context.Set<BankDTO>()
                .FromSqlRaw("SELECT * FROM get_bank_balances(@in_parish_id, @in_start_date::date, @in_end_date::date)",
                    parishIdParam, startDateParam, endDateParam)
                .ToListAsync();

            var bankStatements = rawResults.Select(b => new BankDTO
            {
                BankId = (customizationOption == FinancialReportCustomizationOption.IdsOnly ||
                           customizationOption == FinancialReportCustomizationOption.Both) ? b.BankId : (int?)null,
                BankName = (customizationOption == FinancialReportCustomizationOption.NamesOnly ||
                             customizationOption == FinancialReportCustomizationOption.Both) ? b.BankName : null,
                OpeningBalance = b.OpeningBalance,
                ClosingBalance = b.ClosingBalance,
                Balance = b.Balance
            }).ToList();

            var mappedTransactions = includeTransactions
                ? _mapper.Map<List<FinancialReportCustomDTO>>(transactions, opts =>
                    opts.Items["CustomizationOption"] = customizationOption)
                : new List<FinancialReportCustomDTO>();

            if (includeTransactions)
            {
                var groupedTransactions = mappedTransactions
                    .Where(t => t.BankId.HasValue)
                    .GroupBy(t => t.BankId.Value)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var bank in bankStatements)
                {
                    if (!bank.BankId.HasValue) continue;

                    if (!groupedTransactions.TryGetValue(bank.BankId.Value, out var bankTransactions))
                        continue;

                    // If bank name is "Cash", filter only those with HeadName == "Contra"
                    if (bank.BankName?.Equals("Cash", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        bankTransactions = bankTransactions.Where(t => t.HeadName == "Contra").ToList();
                    }

                    var orderedTransactions = new List<BankFinancialReportCustomDTO>();
                    decimal runningBalance = bank.OpeningBalance;
                    int order = 1;

                    // Add Opening Balance row explicitly
                    orderedTransactions.Add(new BankFinancialReportCustomDTO
                    {
                        TransactionId = 0,
                        TrDate = startDateValue,
                        VrNo = "OB",
                        TransactionType = "Opening Balance",
                        IncomeAmount = 0,
                        ExpenseAmount = 0,
                        Description = "Opening Balance",
                        BillName = null,
                        HeadId = null,
                        HeadName = null,
                        FamilyId = null,
                        FamilyName = null,
                        FamilyNumber = null,
                        BankId = bank.BankId,
                        BankName = bank.BankName,
                        Odr = order++,
                        RunningBalance = runningBalance
                    });

                    foreach (var t in bankTransactions.OrderBy(t => t.TrDate))
                    {
                        var item = new BankFinancialReportCustomDTO
                        {
                            TransactionId = t.TransactionId,
                            TrDate = t.TrDate,
                            VrNo = t.VrNo,
                            TransactionType = t.TransactionType,
                            IncomeAmount = t.IncomeAmount,
                            ExpenseAmount = t.ExpenseAmount,
                            Description = t.Description,
                            BillName = t.BillName,
                            HeadId = t.HeadId,
                            HeadName = t.HeadName,
                            FamilyId = t.FamilyId,
                            FamilyName = t.FamilyName,
                            FamilyNumber = t.FamilyNumber,
                            BankId = t.BankId,
                            BankName = t.BankName,
                            Odr = order++,
                            RunningBalance = runningBalance + t.IncomeAmount - t.ExpenseAmount
                        };

                        runningBalance = item.RunningBalance;
                        orderedTransactions.Add(item);
                    }

                    if (orderedTransactions.Count > 0)
                    {
                        var lastRunningBalance = orderedTransactions.Last().RunningBalance;
                        if (lastRunningBalance != bank.ClosingBalance)//for cash there will be mismatches
                        {
                            Console.WriteLine($"[Warning] Bank '{bank.BankName}' - Closing balance mismatch! Expected: {bank.ClosingBalance}, Calculated: {lastRunningBalance}");
                            // Optionally, log this or throw an exception if critical
                        }
                    }

                    bank.Transactions = orderedTransactions;
                    Console.WriteLine($"Bank: {bank.BankName}, Transactions Count: {bank.Transactions.Count}");


                }
            }

            return new BankStatementConsolidatedDTO
            {
                ParishId = parishId,
                StartDate = startDate,
                EndDate = endDate,
                ReportName = "Bank Statement",
                CurrentBalance = bankStatements.Sum(b => b.Balance),
                Banks = bankStatements
            };
        }
    }
}
