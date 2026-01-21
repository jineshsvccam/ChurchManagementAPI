using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchRepositories.Admin
{
    public class FinancialYearRepository : IFinancialYearRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FinancialYearRepository> _logger;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FinancialYearRepository(
            ApplicationDbContext context,
            ILogger<FinancialYearRepository> logger,
            LogsHelper logsHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _logsHelper = logsHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<FinancialYear>> GetAllAsync(int? parishId)
        {
            _logger.LogInformation("Fetching all financial years for ParishId: {ParishId}", parishId);

            var query = _context.FinancialYears.AsQueryable();

            if (parishId.HasValue)
            {
                query = query.Where(fy => fy.ParishId == parishId.Value);
            }

            var result = await query.ToListAsync();
            _logger.LogInformation("Fetched {Count} financial years.", result.Count);
            return result;
        }

        public async Task<FinancialYear?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching financial year with Id: {Id}", id);
            var financialYear = await _context.FinancialYears.FindAsync(id);

            if (financialYear == null)
            {
                _logger.LogWarning("Financial year with Id: {Id} not found", id);
            }

            return financialYear;
        }

        public async Task<FinancialYear?> GetFinancialYearByDateAsync(int parishId, DateTime date)
        {
            _logger.LogInformation("Fetching financial year for ParishId: {ParishId} and Date: {Date}", parishId, date);

            var financialYear = await _context.FinancialYears
                .Where(fy => fy.ParishId == parishId && fy.StartDate <= date && fy.EndDate >= date)
                .FirstOrDefaultAsync();

            if (financialYear == null)
            {
                _logger.LogWarning("Financial year not found for ParishId: {ParishId} and Date: {Date}", parishId, date);
            }

            return financialYear;
        }

        public async Task<List<FinancialYear>> GetFinancialYearsByDatesAsync(int parishId, List<DateTime> dates)
        {
            _logger.LogInformation("Fetching financial years for ParishId: {ParishId} and Dates: {Dates}", parishId, string.Join(", ", dates));

            var minDate = dates.Min();
            var maxDate = dates.Max();

            var financialYears = await _context.FinancialYears
                .Where(fy => fy.ParishId == parishId &&
                             fy.StartDate <= maxDate &&
                             fy.EndDate >= minDate)
                .ToListAsync();

            return financialYears;
        }

        public async Task<FinancialYear> AddAsync(FinancialYear financialYear)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            _logger.LogInformation("Adding new financial year for ParishId: {ParishId}", financialYear.ParishId);

            financialYear.StartDate = DateTime.SpecifyKind(financialYear.StartDate, DateTimeKind.Utc);
            financialYear.EndDate = DateTime.SpecifyKind(financialYear.EndDate, DateTimeKind.Utc);

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

            _logger.LogInformation("Financial year added with Id: {Id}", financialYear.FinancialYearId);
            return financialYear;
        }

        public async Task<FinancialYear> UpdateAsync(FinancialYear financialYear)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            _logger.LogInformation("Updating financial year with Id: {Id}", financialYear.FinancialYearId);

            var existingFinancialYear = await _context.FinancialYears.FindAsync(financialYear.FinancialYearId);
            if (existingFinancialYear == null)
            {
                _logger.LogWarning("Financial year with Id: {Id} not found", financialYear.FinancialYearId);
                throw new KeyNotFoundException("Financial year not found");
            }

            var oldValues = existingFinancialYear.Clone();

            financialYear.StartDate = DateTime.SpecifyKind(financialYear.StartDate, DateTimeKind.Utc);
            financialYear.EndDate = DateTime.SpecifyKind(financialYear.EndDate, DateTimeKind.Utc);

            _context.Entry(existingFinancialYear).CurrentValues.SetValues(financialYear);
            await _context.SaveChangesAsync();

            await _logsHelper.LogChangeAsync(
                "financial_years",
                financialYear.FinancialYearId,
                "UPDATE",
                userId,
                Extensions.Serialize(oldValues),
                Extensions.Serialize(financialYear)
            );

            _logger.LogInformation("Financial year with Id: {Id} updated successfully", financialYear.FinancialYearId);
            return financialYear;
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            _logger.LogInformation("Deleting financial year with Id: {Id}", id);

            var financialYear = await _context.FinancialYears.FindAsync(id);
            if (financialYear == null)
            {
                _logger.LogWarning("Delete failed: Financial year with Id: {Id} not found", id);
                throw new KeyNotFoundException("Financial year not found");
            }

            _context.FinancialYears.Remove(financialYear);
            await _context.SaveChangesAsync();

            await _logsHelper.LogChangeAsync(
                "financial_years",
                id,
                "DELETE",
                userId,
                Extensions.Serialize(financialYear),
                null
            );

            _logger.LogInformation("Financial year with Id: {Id} deleted successfully", id);
        }
    }
}
