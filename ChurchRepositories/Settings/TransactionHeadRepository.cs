using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories.Settings
{
    public class TransactionHeadRepository : ITransactionHeadRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LogsHelper _logsHelper;

        public TransactionHeadRepository(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            LogsHelper logsHelper)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logsHelper = logsHelper;
        }

        public async Task<IEnumerable<TransactionHead>> GetTransactionHeadsAsync(int? parishId, int? headId)
        {
            var query = _context.TransactionHeads.AsNoTracking().AsQueryable();
            if (parishId.HasValue)
            {
                query = query.Where(th => th.ParishId == parishId.Value);
            }
            if (headId.HasValue)
            {
                query = query.Where(th => th.HeadId == headId.Value);
            }
            return await query.ToListAsync();
        }

        public async Task<TransactionHead?> GetByIdAsync(int id)
        {
            return await _context.TransactionHeads.FindAsync(id);
        }

        public async Task<TransactionHead> AddAsync(TransactionHead transactionHead)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, transactionHead.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            await _context.TransactionHeads.AddAsync(transactionHead);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("transaction_heads", transactionHead.HeadId, "INSERT", userId, null, Extensions.Serialize(transactionHead));
            return transactionHead;
        }

        public async Task<TransactionHead> UpdateAsync(TransactionHead transactionHead)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, transactionHead.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existingTransactionHead = await _context.TransactionHeads.FindAsync(transactionHead.HeadId);
            if (existingTransactionHead == null)
            {
                throw new KeyNotFoundException("Transaction head not found");
            }

            var oldValues = existingTransactionHead.Clone();
            _context.Entry(existingTransactionHead).CurrentValues.SetValues(transactionHead);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("transaction_heads", transactionHead.HeadId, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(transactionHead));
            return transactionHead;
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var transactionHead = await _context.TransactionHeads.FindAsync(id);
            if (transactionHead == null)
            {
                throw new KeyNotFoundException("Transaction head not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, transactionHead.ParishId);

            _context.TransactionHeads.Remove(transactionHead);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("transaction_heads", id, "DELETE", userId, Extensions.Serialize(transactionHead), null);
        }
    }
}
