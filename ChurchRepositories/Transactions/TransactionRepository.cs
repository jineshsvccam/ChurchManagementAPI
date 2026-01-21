using ChurchCommon.Utils;
using ChurchContracts;
using ChurchContracts.Utils;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ChurchRepositories.Transactions
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransactionRepository(ApplicationDbContext context, IMemoryCache cache, LogsHelper logsHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _cache = cache;
            _logsHelper = logsHelper;
            _httpContextAccessor = httpContextAccessor;
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
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, transaction.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("transactions", transaction.TransactionId, "INSERT", userId, null, Extensions.Serialize(transaction));
            _context.Entry(transaction).State = EntityState.Detached;
            return transaction;
        }

        public async Task<Transaction> UpdateAsync(Transaction transaction)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, transaction.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existingTransaction = await _context.Transactions.FindAsync(transaction.TransactionId);
            if (existingTransaction == null)
            {
                throw new KeyNotFoundException("Transaction not found");
            }

            var oldValues = existingTransaction.Clone();
            _context.Entry(existingTransaction).CurrentValues.SetValues(transaction);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("transactions", transaction.TransactionId, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(transaction));
            return transaction;
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                throw new KeyNotFoundException("Transaction not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, transaction.ParishId);

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("transactions", transaction.TransactionId, "DELETE", userId, Extensions.Serialize(transaction), null);
        }

        public async Task<List<Transaction>> GetByIdsAsync(int[] ids)
        {
            return await _context.Transactions
                .Where(t => ids.Contains(t.TransactionId))
                .ToListAsync();
        }

        public async Task DeleteMultipleAsync(int[] ids)
        {
            using (var dbTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);

                    // Fetch the transactions to delete
                    var transactions = await _context.Transactions
                        .Where(t => ids.Contains(t.TransactionId))
                        .ToListAsync();

                    // Verify that we found all requested IDs
                    if (transactions.Count != ids.Length)
                    {
                        var foundIds = transactions.Select(t => t.TransactionId).ToHashSet();
                        var missingIds = ids.Where(id => !foundIds.Contains(id));
                        throw new InvalidOperationException(
                            $"Some transactions were not found for deletion: {string.Join(", ", missingIds)}");
                    }

                    // Validate parish ownership for all transactions
                    foreach (var transaction in transactions)
                    {
                        await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, transaction.ParishId);
                    }

                    // Delete all transactions and log each deletion
                    foreach (var transaction in transactions)
                    {
                        _context.Transactions.Remove(transaction);
                        await _logsHelper.LogChangeAsync("transactions", transaction.TransactionId, "DELETE", userId, Extensions.Serialize(transaction), null);
                    }

                    int deletedCount = await _context.SaveChangesAsync();

                    // Verify that all records were actually deleted
                    if (deletedCount != ids.Length)
                    {
                        throw new InvalidOperationException(
                            $"Expected to delete {ids.Length} transactions, but only {deletedCount} were deleted.");
                    }

                    // Commit the transaction if everything succeeded
                    await dbTransaction.CommitAsync();
                }
                catch (Exception)
                {
                    // Roll back the transaction on any failure
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
