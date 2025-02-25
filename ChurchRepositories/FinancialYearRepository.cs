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


        public async Task<FinancialYear?> GetByIdAsync(int financialYearId)
        {
            return await _context.FinancialYears.FindAsync(financialYearId);
        }

        public async Task<FinancialYear> AddAsync(FinancialYear financialYear)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            // Ensure DateTime values are stored as UTC
            financialYear.StartDate = DateTime.SpecifyKind(financialYear.StartDate, DateTimeKind.Utc);
            financialYear.EndDate = DateTime.SpecifyKind(financialYear.EndDate, DateTimeKind.Utc);

            await _context.FinancialYears.AddAsync(financialYear);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("financial_years", financialYear.FinancialYearId, "INSERT", userId, null, Extensions.Serialize(financialYear));
            return financialYear;
        }

        public async Task<FinancialYear> UpdateAsync(FinancialYear financialYear)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existing = await _context.FinancialYears.FindAsync(financialYear.FinancialYearId);
            if (existing != null)
            {
                var oldValues = existing.Clone();
                // Ensure DateTime values are stored as UTC
                financialYear.StartDate = DateTime.SpecifyKind(financialYear.StartDate, DateTimeKind.Utc);
                financialYear.EndDate = DateTime.SpecifyKind(financialYear.EndDate, DateTimeKind.Utc);

                _context.FinancialYears.Update(financialYear);
                await _context.SaveChangesAsync();
                await _logsHelper.LogChangeAsync("financial_years", financialYear.FinancialYearId, "INSERT", userId, Extensions.Serialize(oldValues), Extensions.Serialize(financialYear));
                return financialYear;
            }
            else
            {
                throw new KeyNotFoundException("FinancialYear not found");
            }
        }

        public async Task DeleteAsync(int financialYearId)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var financialYear = await _context.FinancialYears.FindAsync(financialYearId);
            if (financialYear != null)
            {
                _context.FinancialYears.Remove(financialYear);
                await _context.SaveChangesAsync();
                await _logsHelper.LogChangeAsync("financial_years", financialYear.FinancialYearId, "DELETE", userId, null, Extensions.Serialize(financialYear));
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
