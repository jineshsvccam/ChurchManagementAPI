using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;
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

        public async Task<IEnumerable<LedgerReportDTO>> GetLedgerAsync(int parishId, DateTime? startDate, DateTime? endDate, bool includeTransactions = false)
        {
            var query = _context.FinancialReportsView
                .Where(r => r.ParishId == parishId) // Filter by Parish ID
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(r => r.TrDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.TrDate <= endDate.Value);
            }

            var transactions = await query.ToListAsync();

            var groupedTransactions = transactions
                .GroupBy(t => t.HeadName)
                .Select(g => new HeadDTO
                {
                    HeadName = g.Key,
                    IncomeAmount = g.Sum(t => t.IncomeAmount),
                    ExpenseAmount = g.Sum(t => t.ExpenseAmount),
                    Balance = g.Sum(t => t.IncomeAmount) - g.Sum(t => t.ExpenseAmount)
                }).ToList();

            var report = new LedgerReportDTO
            {
                Heads = groupedTransactions,
                Transactions = includeTransactions ? transactions : new List<FinancialReportsView>()
            };

            return new List<LedgerReportDTO> { report };
        }
    }
}
