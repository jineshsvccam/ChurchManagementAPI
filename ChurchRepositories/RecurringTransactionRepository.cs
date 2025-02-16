using ChurchContracts;
using ChurchData;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchRepositories
{
    public class RecurringTransactionRepository : IRecurringTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public RecurringTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RecurringTransaction>> GetAllAsync(int? parishId)
        {
            var query = _context.RecurringTransactions.AsQueryable();
            if (parishId.HasValue)
                query = query.Where(rt => rt.ParishId == parishId.Value);
            return await query.ToListAsync();
        }

        public async Task<RecurringTransaction?> GetByIdAsync(int id)
        {
            return await _context.RecurringTransactions.FindAsync(id);
        }

        public async Task<RecurringTransaction> AddAsync(RecurringTransaction recurringTransaction)
        {
            await _context.RecurringTransactions.AddAsync(recurringTransaction);
            await _context.SaveChangesAsync();
            return recurringTransaction;
        }

        public async Task<RecurringTransaction> UpdateAsync(RecurringTransaction recurringTransaction)
        {
            var existingTransaction = await _context.RecurringTransactions.FindAsync(recurringTransaction.RepeatedEntryId);
            if (existingTransaction != null)
            {
                _context.Entry(existingTransaction).CurrentValues.SetValues(recurringTransaction);
                await _context.SaveChangesAsync();
                return recurringTransaction;
            }
            else
            {
                throw new KeyNotFoundException("Recurring Transaction not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var transaction = await _context.RecurringTransactions.FindAsync(id);
            if (transaction != null)
            {
                _context.RecurringTransactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Recurring Transaction not found");
            }
        }
    }
}
