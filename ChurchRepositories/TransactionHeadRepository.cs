using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;
using ChurchRepositories.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChurchRepositories
{
    public class TransactionHeadRepository : ITransactionHeadRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IOptions<LoggingSettings> _loggingSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        public TransactionHeadRepository(ApplicationDbContext context, IOptions<LoggingSettings> loggingSettings, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _loggingSettings = loggingSettings;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<TransactionHead>> GetTransactionHeadsAsync(int? parishId, int? headId)
        {
            var query = _context.TransactionHeads.AsQueryable();

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
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor); 

            await _context.TransactionHeads.AddAsync(transactionHead);
            await _context.SaveChangesAsync();

            // Log the addition
            await LogChangeAsync("transaction_heads", transactionHead.HeadId, "INSERT", userId, null, transactionHead);

            return transactionHead;
        }

        public async Task<TransactionHead> UpdateAsync(TransactionHead transactionHead)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existingTransactionHead = await _context.TransactionHeads
                .AsNoTracking() // Ensure no tracking to capture the old values correctly
                .FirstOrDefaultAsync(th => th.HeadId == transactionHead.HeadId);

            if (existingTransactionHead != null)
            {
                var oldValues = CloneTransactionHead(existingTransactionHead); // Clone old values

                _context.TransactionHeads.Update(transactionHead);
                await _context.SaveChangesAsync();

                // Log the update with userId
                await LogChangeAsync("transaction_heads", transactionHead.HeadId, "UPDATE", userId, oldValues, transactionHead);

                return transactionHead;
            }
            else
            {
                throw new KeyNotFoundException("TransactionHead not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var transactionHead = await _context.TransactionHeads.FindAsync(id);
            if (transactionHead != null)
            {
                _context.TransactionHeads.Remove(transactionHead);
                await _context.SaveChangesAsync();

                // Log the deletion
                await LogChangeAsync("transaction_heads", id, "DELETE", userId, transactionHead, null);
            }
            else
            {
                throw new KeyNotFoundException("TransactionHead not found");
            }
        }

        private async Task LogChangeAsync(string tableName, int recordId, string changeType, int userId, TransactionHead? oldValues, TransactionHead? newValues)
        {
            if (!_loggingSettings.Value.EnableChangeLogging ||
                !_loggingSettings.Value.TableLogging.ContainsKey(tableName) ||
                !_loggingSettings.Value.TableLogging[tableName])
            {
                return; // Logging is disabled for this table
            }

            var log = new GenericLog
            {
                TableName = tableName,
                RecordId = recordId,
                ChangeType = changeType,
                ChangedBy = userId,
                ChangeTimestamp = DateTime.UtcNow, // Ensure UTC
                OldValues = oldValues != null ? SerializeTransactionHead(oldValues) : null,
                NewValues = newValues != null ? SerializeTransactionHead(newValues) : null
            };

            await _context.GenericLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        // Serialize without navigation properties
        private string SerializeTransactionHead(TransactionHead transactionHead)
        {
            var transactionHeadDto = new
            {
                transactionHead.HeadId,
                transactionHead.ParishId,
                transactionHead.HeadName,
                transactionHead.Type,
                transactionHead.Description
            };

            return Newtonsoft.Json.JsonConvert.SerializeObject(transactionHeadDto);
        }

        private TransactionHead CloneTransactionHead(TransactionHead source)
        {
            return new TransactionHead
            {
                HeadId = source.HeadId,
                HeadName = source.HeadName,
                Type = source.Type,
                IsMandatory = source.IsMandatory,
                Description = source.Description,
                ParishId = source.ParishId,
                Aramanapct = source.Aramanapct,
                Ordr = source.Ordr,
                HeadNameMl = source.HeadNameMl,
                Transactions = source.Transactions.Select(t => new Transaction { /* Copy transaction properties here */ }).ToList() // Clone the list
            };
        }
    }
}
