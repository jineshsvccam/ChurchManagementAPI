using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(int? parishId, int? familyId, int? transactionId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Transactions.AsQueryable();

            if (parishId.HasValue)
            {
                query = query.Where(t => t.ParishId == parishId.Value);
            }

            if (familyId.HasValue)
            {
                query = query.Where(t => t.FamilyId == familyId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.TrDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.TrDate <= endDate.Value);
            }

            if (transactionId.HasValue)
            {
                query = query.Where(t => t.TransactionId == transactionId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task<Transaction> AddAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction> UpdateAsync(Transaction transaction)
        {
            var existingTransaction = await _context.Transactions.FindAsync(transaction.TransactionId);
            if (existingTransaction != null)
            {
                _context.Entry(existingTransaction).CurrentValues.SetValues(transaction);
                await _context.SaveChangesAsync();
                return transaction;
            }
            else
            {
                throw new KeyNotFoundException("Transaction not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Transaction not found");
            }
        }
    }
}
