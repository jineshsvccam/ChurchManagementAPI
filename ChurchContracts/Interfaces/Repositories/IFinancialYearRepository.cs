using System;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface IFinancialYearRepository
    {
        Task<FinancialYear?> GetFinancialYearByDateAsync(int parishId, DateTime date);
        Task<FinancialYear?> GetByIdAsync(int financialYearId);
        Task<FinancialYear> AddAsync(FinancialYear financialYear);
        Task<FinancialYear> UpdateAsync(FinancialYear financialYear);
        Task DeleteAsync(int financialYearId);
        Task<IEnumerable<FinancialYear>> GetAllAsync();
    }
}
