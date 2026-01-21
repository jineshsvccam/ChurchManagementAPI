using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchRepositories.Transactions
{
    public class RecurringTransactionRepository : IRecurringTransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RecurringTransactionRepository(ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            LogsHelper logsHelper)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logsHelper = logsHelper;
        }

        public async Task<IEnumerable<RecurringTransaction>> GetAllAsync(int? parishId)
        {
            var query = _context.RecurringTransactions.AsNoTracking().AsQueryable();
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
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, recurringTransaction.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            await _context.RecurringTransactions.AddAsync(recurringTransaction);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("recurring_transactions", recurringTransaction.RepeatedEntryId, "INSERT", userId, null, Extensions.Serialize(recurringTransaction));
            return recurringTransaction;
        }

        public async Task<RecurringTransaction> UpdateAsync(RecurringTransaction recurringTransaction)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, recurringTransaction.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existingTransaction = await _context.RecurringTransactions.FindAsync(recurringTransaction.RepeatedEntryId);
            if (existingTransaction == null)
            {
                throw new KeyNotFoundException("Recurring Transaction not found");
            }

            var oldValues = existingTransaction.Clone();
            _context.Entry(existingTransaction).CurrentValues.SetValues(recurringTransaction);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("recurring_transactions", recurringTransaction.RepeatedEntryId, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(recurringTransaction));
            return recurringTransaction;
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var transaction = await _context.RecurringTransactions.FindAsync(id);
            if (transaction == null)
            {
                throw new KeyNotFoundException("Recurring Transaction not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, transaction.ParishId);

            _context.RecurringTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("recurring_transactions", transaction.RepeatedEntryId, "DELETE", userId, Extensions.Serialize(transaction), null);
        }

        public async Task<int> DeleteByParishAndHeadAsync(int parishId, int headId)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, parishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var transactionsToDelete = await _context.RecurringTransactions
                .Where(rt => rt.ParishId == parishId && rt.HeadId == headId)
                .ToListAsync();

            if (!transactionsToDelete.Any())
            {
                throw new KeyNotFoundException($"No recurring transactions found for ParishId={parishId} and HeadId={headId}");
            }

            foreach (var transaction in transactionsToDelete)
            {
                _context.RecurringTransactions.Remove(transaction);
                await _logsHelper.LogChangeAsync("recurring_transactions", transaction.RepeatedEntryId, "DELETE", userId, Extensions.Serialize(transaction), null);
            }

            await _context.SaveChangesAsync();

            return transactionsToDelete.Count;
        }
    }
}
