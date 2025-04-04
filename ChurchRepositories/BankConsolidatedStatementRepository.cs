using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using Microsoft.EntityFrameworkCore;
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
            // Convert the start and end dates to UTC; if not provided, use default extremes.
            var startDateValue = startDate.HasValue
                ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                : DateTime.MinValue;
            var endDateValue = endDate.HasValue
                ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
                : DateTime.MaxValue;

            // Build query for transactions
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

            // Setup SQL parameters for the raw SQL call
            var parishIdParam = new NpgsqlParameter<int>("in_parish_id", parishId);
            var startDateParam = new NpgsqlParameter<DateTime>("in_start_date", startDateValue.Date);
            var endDateParam = new NpgsqlParameter<DateTime>("in_end_date", endDateValue.Date);

            // Execute the stored procedure to get bank balances.
            var rawResults = await _context.Set<BankDTO>()
                 .FromSqlRaw("SELECT * FROM get_bank_balances(@in_parish_id, @in_start_date::date, @in_end_date::date)",
                             parishIdParam, startDateParam, endDateParam)
                 .ToListAsync();

            // Optional: Log raw results for debugging.
            foreach (var result in rawResults)
            {
                Console.WriteLine($"BankName: {result.BankName}, OpeningBalance: {result.OpeningBalance}, ClosingBalance: {result.ClosingBalance}, Balance: {result.Balance}");
            }

            // Map the raw results to your BankDTO based on the customization option.
            var bankStatements = rawResults.Select(b => new BankDTO
            {
                // Include BankId if the option is IdsOnly or Both; otherwise, set to null.
                BankId = (customizationOption == FinancialReportCustomizationOption.IdsOnly ||
                           customizationOption == FinancialReportCustomizationOption.Both) ? b.BankId : (int?)null,
                // Include BankName if the option is NamesOnly or Both; otherwise, set to null.
                BankName = (customizationOption == FinancialReportCustomizationOption.NamesOnly ||
                             customizationOption == FinancialReportCustomizationOption.Both) ? b.BankName : null,
                OpeningBalance = b.OpeningBalance,
                ClosingBalance = b.ClosingBalance,
                Balance = b.Balance
            }).ToList();

            // Use AutoMapper to map transactions into FinancialReportCustomDTO list.
            var mappedTransactions = includeTransactions
                 ? _mapper.Map<List<FinancialReportCustomDTO>>(transactions, opts =>
                    opts.Items["CustomizationOption"] = customizationOption)
                 : new List<FinancialReportCustomDTO>();

            return new BankStatementConsolidatedDTO
            {
                ParishId = parishId,
                StartDate = startDate,
                EndDate = endDate,
                ReportName = "Bank Statement",             
                CurrentBalance = bankStatements.Sum(b => b.Balance),
                Banks = bankStatements,
                Transactions = mappedTransactions
            };
        }
    }
}
