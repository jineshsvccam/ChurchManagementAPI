using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData.DTOs;
using ChurchData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChurchRepositories.Utils
{
    public class LogsHelper
    {
        private readonly ApplicationDbContext _context;  // Injected instead of passing as a parameter
        private readonly IOptions<LoggingSettings> _loggingSettings;
        private readonly ILogger<LogsHelper> _logger;

        public LogsHelper(ApplicationDbContext context, IOptions<LoggingSettings> loggingSettings, ILogger<LogsHelper> logger)
        {
            _context = context;
            _loggingSettings = loggingSettings;
            _logger = logger;
        }

        public async Task LogChangeAsync(string tableName, int recordId, string changeType, int userId, string oldValues, string newValues)
        {
            try
            {
                if (!_loggingSettings.Value.EnableChangeLogging ||
                    !_loggingSettings.Value.TableLogging.ContainsKey(tableName) ||
                    !_loggingSettings.Value.TableLogging[tableName])
                {
                    return;
                }

                _logger.LogInformation("DB logging begins for the table: {TableName}", tableName);

                var log = new GenericLog
                {
                    TableName = tableName,
                    RecordId = recordId,
                    ChangeType = changeType,
                    ChangedBy = userId,
                    ChangeTimestamp = DateTime.UtcNow,
                    OldValues = oldValues,
                    NewValues = newValues
                };

                await _context.GenericLogs.AddAsync(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Logged change for table: {TableName}, RecordId: {RecordId}, ChangeType: {ChangeType}", tableName, recordId, changeType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging change for table: {TableName}, RecordId: {RecordId}, ChangeType: {ChangeType}", tableName, recordId, changeType);
            }
        }
    }

}
