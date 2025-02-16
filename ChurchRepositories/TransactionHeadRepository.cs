using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChurchRepositories
{
    public class TransactionHeadRepository : ITransactionHeadRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IOptions<LoggingSettings> _loggingSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TransactionHeadRepository> _logger;
        private readonly LogsHelper _logsHelper;

        public TransactionHeadRepository(
            ApplicationDbContext context,
            IOptions<LoggingSettings> loggingSettings,
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TransactionHeadRepository> logger,
            LogsHelper logsHelper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _loggingSettings = loggingSettings ?? throw new ArgumentNullException(nameof(loggingSettings));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logsHelper = logsHelper;
        }

        public async Task<IEnumerable<TransactionHead>> GetTransactionHeadsAsync(int? parishId, int? headId)
        {
            _logger.LogInformation("Fetching transaction heads for ParishId: {ParishId}, HeadId: {HeadId}", parishId, headId);

            var query = _context.TransactionHeads.AsQueryable();
            if (parishId.HasValue)
                query = query.Where(th => th.ParishId == parishId.Value);
            if (headId.HasValue)
                query = query.Where(th => th.HeadId == headId.Value);

            var result = await query.ToListAsync();
            _logger.LogInformation("Fetched {Count} transaction heads.", result.Count);

            return result;
        }

        public async Task<TransactionHead?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching transaction head by Id: {Id}", id);
            var result = await _context.TransactionHeads.FindAsync(id);

            if (result == null)
                _logger.LogWarning("Transaction head not found with Id: {Id}", id);

            return result;
        }

        public async Task<TransactionHead> AddAsync(TransactionHead transactionHead)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            _logger.LogInformation("Adding transaction head: {HeadName}", transactionHead.HeadName);

            await _context.TransactionHeads.AddAsync(transactionHead);
            await _context.SaveChangesAsync();

            await _logsHelper.LogChangeAsync("transaction_heads", transactionHead.HeadId, "INSERT", userId, null, SerializeTransactionHead(transactionHead));

            _logger.LogInformation("Successfully added transaction head Id: {Id}", transactionHead.HeadId);
            return transactionHead;
        }

        public async Task<TransactionHead> UpdateAsync(TransactionHead transactionHead)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            _logger.LogInformation("Updating transaction head Id: {Id}", transactionHead.HeadId);

            var existingTransactionHead = await _context.TransactionHeads
                .AsNoTracking()
                .FirstOrDefaultAsync(th => th.HeadId == transactionHead.HeadId);

            if (existingTransactionHead == null)
            {
                _logger.LogWarning("Transaction head not found for update with Id: {Id}", transactionHead.HeadId);
                throw new KeyNotFoundException("TransactionHead not found");
            }

            var oldValues = CloneTransactionHead(existingTransactionHead);

            _context.TransactionHeads.Update(transactionHead);
            await _context.SaveChangesAsync();

            await _logsHelper.LogChangeAsync("transaction_heads", transactionHead.HeadId, "UPDATE", userId, SerializeTransactionHead(oldValues), SerializeTransactionHead(transactionHead));

            _logger.LogInformation("Successfully updated transaction head Id: {Id}", transactionHead.HeadId);
            return transactionHead;
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            _logger.LogInformation("Deleting transaction head Id: {Id}", id);

            var transactionHead = await _context.TransactionHeads.FindAsync(id);
            if (transactionHead == null)
            {
                _logger.LogWarning("Transaction head not found for deletion with Id: {Id}", id);
                throw new KeyNotFoundException("TransactionHead not found");
            }

            _context.TransactionHeads.Remove(transactionHead);
            await _context.SaveChangesAsync();

            await _logsHelper.LogChangeAsync("transaction_heads", id, "DELETE", userId, SerializeTransactionHead(transactionHead), null);

            _logger.LogInformation("Successfully deleted transaction head Id: {Id}", id);
        }

        private string SerializeTransactionHead(TransactionHead transactionHead)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                transactionHead.HeadId,
                transactionHead.ParishId,
                transactionHead.HeadName,
                transactionHead.Type,
                transactionHead.Description
            });
        }

        private TransactionHead CloneTransactionHead(TransactionHead source)
        {
            return new TransactionHead
            {
                HeadId = source.HeadId,
                ParishId = source.ParishId,
                HeadName = source.HeadName,
                Type = source.Type,
                IsMandatory = source.IsMandatory,
                Description = source.Description,
                Aramanapct = source.Aramanapct,
                Ordr = source.Ordr,
                HeadNameMl = source.HeadNameMl
            };
        }
    }
}
