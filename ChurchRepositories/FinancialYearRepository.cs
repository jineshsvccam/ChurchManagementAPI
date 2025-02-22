using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class FinancialYearRepository : IFinancialYearRepository
    {
        private readonly ApplicationDbContext _context;

        public FinancialYearRepository(ApplicationDbContext context)
        {
            _context = context;
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
            // Ensure DateTime values are stored as UTC
            financialYear.StartDate = DateTime.SpecifyKind(financialYear.StartDate, DateTimeKind.Utc);
            financialYear.EndDate = DateTime.SpecifyKind(financialYear.EndDate, DateTimeKind.Utc);

            await _context.FinancialYears.AddAsync(financialYear);
            await _context.SaveChangesAsync();
            return financialYear;
        }

        public async Task<FinancialYear> UpdateAsync(FinancialYear financialYear)
        {
            // Ensure DateTime values are stored as UTC
            financialYear.StartDate = DateTime.SpecifyKind(financialYear.StartDate, DateTimeKind.Utc);
            financialYear.EndDate = DateTime.SpecifyKind(financialYear.EndDate, DateTimeKind.Utc);

            _context.FinancialYears.Update(financialYear);
            await _context.SaveChangesAsync();
            return financialYear;
        }

        public async Task DeleteAsync(int financialYearId)
        {
            var financialYear = await _context.FinancialYears.FindAsync(financialYearId);
            if (financialYear != null)
            {
                _context.FinancialYears.Remove(financialYear);
                await _context.SaveChangesAsync();
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
