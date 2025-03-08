using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchRepositories
{
    public class FinancialYearRepository : IFinancialYearRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TransactionHeadRepository> _logger;

        public FinancialYearRepository(ApplicationDbContext context,
                               IHttpContextAccessor httpContextAccessor,
                               ILogger<TransactionHeadRepository> logger,
                                LogsHelper logsHelper)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logsHelper = logsHelper;
            _logger = logger;
        }


        public async Task<FinancialYear?> GetFinancialYearByDateAsync(int parishId, DateTime date)
        {
            // Ensure the date is in UTC
            DateTime utcDate = date.ToUniversalTime();

            return await _context.FinancialYears
                .FirstOrDefaultAsync(fy => fy.ParishId == parishId && fy.StartDate <= utcDate && fy.EndDate >= utcDate);
        }
        public async Task<List<FinancialYear>> GetFinancialYearsByDatesAsync(int parishId, List<DateTime> dates)
        {
            // Convert all dates to UTC using the same logic as GetFinancialYearByDateAsync
            var utcDates = dates.Select(date => date.ToUniversalTime()).ToList();

            return await _context.FinancialYears
                .Where(fy => fy.ParishId == parishId &&
                            utcDates.Any(d => d >= fy.StartDate && d <= fy.EndDate))
                .Distinct()  // Add Distinct in case date ranges overlap with multiple financial years
                .ToListAsync();
        }


        public async Task<FinancialYear?> GetByIdAsync(int financialYearId)
        {
            return await _context.FinancialYears.FindAsync(financialYearId);
        }
        public async Task<FinancialYear> AddAsync(FinancialYear financialYear)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);

            // Ensure required fields are set
            if (financialYear.StartDate == default || financialYear.EndDate == default)
                throw new ArgumentException("StartDate and EndDate are required.");

            // Ensure StartDate is before EndDate
            if (financialYear.StartDate >= financialYear.EndDate)
                throw new ArgumentException("StartDate must be earlier than EndDate.");

            // Convert Dates to UTC
            financialYear.StartDate = DateTime.SpecifyKind(financialYear.StartDate, DateTimeKind.Utc);
            financialYear.EndDate = DateTime.SpecifyKind(financialYear.EndDate, DateTimeKind.Utc);

            if (financialYear.LockDate.HasValue)
            {
                financialYear.LockDate = DateTime.SpecifyKind(financialYear.LockDate.Value, DateTimeKind.Utc);
            }


            // Ensure ParishId exists
            bool parishExists = await _context.Parishes.AnyAsync(p => p.ParishId == financialYear.ParishId);
            if (!parishExists)
                throw new KeyNotFoundException($"Parish with ID {financialYear.ParishId} not found.");

            // Check for duplicate financial years (Example: Unique per Parish)
            bool exists = await _context.FinancialYears
                .AnyAsync(fy => fy.ParishId == financialYear.ParishId && fy.StartDate == financialYear.StartDate);
            if (exists)
                throw new InvalidOperationException("A financial year with the same period already exists for this parish.");

            // Prevent manual ID insertion
            financialYear.FinancialYearId = 0;

           
            await _context.FinancialYears.AddAsync(financialYear);
            await _context.SaveChangesAsync();

            await _logsHelper.LogChangeAsync(
                "financial_years",
                financialYear.FinancialYearId,
                "INSERT",
                userId,
                null,
                Extensions.Serialize(financialYear)
            );

            return financialYear;
        }

        public async Task<FinancialYear> UpdateAsync(FinancialYear financialYear)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);

            // Ensure required fields are set
            if (financialYear.StartDate == default || financialYear.EndDate == default)
                throw new ArgumentException("StartDate and EndDate are required.");

            // Ensure StartDate is before EndDate
            if (financialYear.StartDate >= financialYear.EndDate)
                throw new ArgumentException("StartDate must be earlier than EndDate.");

            // Retrieve existing record
            var existing = await _context.FinancialYears.FindAsync(financialYear.FinancialYearId);
            if (existing == null)
                throw new KeyNotFoundException("FinancialYear not found.");

            // Ensure ParishId exists
            bool parishExists = await _context.Parishes.AnyAsync(p => p.ParishId == financialYear.ParishId);
            if (!parishExists)
                throw new KeyNotFoundException($"Parish with ID {financialYear.ParishId} not found.");
           
            // Convert Dates to UTC
            financialYear.StartDate = DateTime.SpecifyKind(financialYear.StartDate, DateTimeKind.Utc);
            financialYear.EndDate = DateTime.SpecifyKind(financialYear.EndDate, DateTimeKind.Utc);

            if (financialYear.LockDate.HasValue)
            {
                financialYear.LockDate = DateTime.SpecifyKind(financialYear.LockDate.Value, DateTimeKind.Utc);
            }

            // Check for duplicate financial years (Ensure no duplicate StartDate for same Parish)
            bool exists = await _context.FinancialYears
                .AnyAsync(fy => fy.ParishId == financialYear.ParishId &&
                                fy.StartDate == financialYear.StartDate &&
                                fy.FinancialYearId != financialYear.FinancialYearId);
            if (exists)
                throw new InvalidOperationException("A financial year with the same period already exists for this parish.");

            // Clone old values for logging
            var oldValues = existing.Clone();

         

            // Apply the updates
            _context.Entry(existing).CurrentValues.SetValues(financialYear);
            await _context.SaveChangesAsync();

            // Log changes
            await _logsHelper.LogChangeAsync(
                "financial_years",
                financialYear.FinancialYearId,
                "UPDATE",
                userId,
                Extensions.Serialize(oldValues),
                Extensions.Serialize(financialYear)
            );

            return financialYear;
        }

        public async Task DeleteAsync(int financialYearId)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var financialYear = await _context.FinancialYears.FindAsync(financialYearId);
            if (financialYear != null)
            {
                _context.FinancialYears.Remove(financialYear);
                await _context.SaveChangesAsync();
                await _logsHelper.LogChangeAsync("financial_years", financialYear.FinancialYearId, "DELETE", userId, Extensions.Serialize(financialYear), null);
            }
        }

        public async Task<IEnumerable<FinancialYear>> GetAllAsync(int? parishId)
        {
            var query = _context.FinancialYears.AsQueryable();

            if (parishId.HasValue)
            {
                query = query.Where(f => f.ParishId == parishId.Value);
            }

            var result = await query.ToListAsync();

            return result;
        }
    }
}
