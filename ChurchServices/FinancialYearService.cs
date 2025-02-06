using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;

namespace ChurchServices
{
    public class FinancialYearService : IFinancialYearService
    {
        private readonly IFinancialYearRepository _financialYearRepository;

        public FinancialYearService(IFinancialYearRepository financialYearRepository)
        {
            _financialYearRepository = financialYearRepository;
        }

        public async Task<FinancialYear?> GetFinancialYearByDateAsync(int parishId, DateTime date)
        {
            return await _financialYearRepository.GetFinancialYearByDateAsync(parishId, date);
        }

        public async Task<FinancialYear?> GetByIdAsync(int financialYearId)
        {
            return await _financialYearRepository.GetByIdAsync(financialYearId);
        }

        public async Task<FinancialYear> AddAsync(FinancialYear financialYear)
        {
            return await _financialYearRepository.AddAsync(financialYear);
        }

        public async Task<FinancialYear> UpdateAsync(FinancialYear financialYear)
        {
            return await _financialYearRepository.UpdateAsync(financialYear);
        }

        public async Task DeleteAsync(int financialYearId)
        {
            await _financialYearRepository.DeleteAsync(financialYearId);
        }

        public async Task<IEnumerable<FinancialYear>> GetAllAsync()
        {
            return await _financialYearRepository.GetAllAsync();
        }

        public async Task LockFinancialYearAsync(int financialYearId)
        {
            var financialYear = await _financialYearRepository.GetByIdAsync(financialYearId);
            if (financialYear == null)
            {
                throw new InvalidOperationException("Financial year not found.");
            }

            financialYear.IsLocked = true;
            financialYear.LockDate = DateTime.UtcNow;

            await _financialYearRepository.UpdateAsync(financialYear);
        }
    }
}
