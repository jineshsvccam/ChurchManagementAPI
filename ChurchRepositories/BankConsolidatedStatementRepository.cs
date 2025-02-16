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

        public BankConsolidatedStatementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BankStatementConsolidatedDTO> GetBankStatementAsync(int parishId, DateTime? startDate, DateTime? endDate, bool includeTransactions = false)
        {
            var startDateValue = startDate.HasValue ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc) : DateTime.MinValue;
            var endDateValue = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc) : DateTime.MaxValue;

            var query = _context.FinancialReportsView
                .Where(r => r.ParishId == parishId && r.BankId != null && r.BankName != "Cash" && r.BankName != "JE")
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

            // Log the raw SQL results
            var rawResults = await _context.Set<BankDTO>()
                .FromSqlRaw("SELECT * FROM get_bank_balances(@in_parish_id, @in_start_date::date, @in_end_date::date)", parishIdParam, startDateParam, endDateParam)
                .ToListAsync();

            foreach (var result in rawResults)
            {
                Console.WriteLine($"BankName: {result.BankName}, OpeningBalance: {result.OpeningBalance}, ClosingBalance: {result.ClosingBalance}, Balance: {result.Balance}");
            }

            var bankStatements = rawResults.Select(b => new BankDTO
            {
                BankName = b.BankName,
                OpeningBalance = b.OpeningBalance,
                ClosingBalance = b.ClosingBalance,
                Balance = b.Balance
            }).ToList();

            return new BankStatementConsolidatedDTO
            {
                Banks = bankStatements,
                Transactions = includeTransactions ? transactions : new List<FinancialReportsView>()
            };
        }
    }
}
