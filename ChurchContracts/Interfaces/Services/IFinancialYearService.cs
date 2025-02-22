using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IFinancialYearService
    {
        Task<FinancialYearDto?> GetFinancialYearByDateAsync(int parishId, DateTime date);
        Task<FinancialYearDto?> GetByIdAsync(int financialYearId);
        Task<FinancialYearDto> AddAsync(FinancialYearDto financialYearDto);
        Task<FinancialYearDto> UpdateAsync(FinancialYearDto financialYearDto);
        Task DeleteAsync(int financialYearId);
        Task<IEnumerable<FinancialYearDto>> GetAllAsync(int? parishId);
        Task LockFinancialYearAsync(int financialYearId);
    }
}
