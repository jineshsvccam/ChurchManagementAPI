using ChurchContracts;
using ChurchContracts.Utils;
using ChurchData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ChurchRepositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public TransactionRepository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<PagedResult<Transaction>> GetTransactionsAsync(int? parishId, int? familyId, int? transactionId, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            var cacheKey = $"GetTransactionsAsync-{parishId}-{familyId}-{transactionId}-{startDate}-{endDate}-{pageNumber}-{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out PagedResult<Transaction> transactions))
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

                var totalCount = await query.CountAsync();
                var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

                transactions = new PagedResult<Transaction>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    PageNumber = pageNumber
                };

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, transactions, cacheEntryOptions);
            }

            return transactions;
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
