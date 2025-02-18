using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    namespace ChurchContracts
    {
        public interface IBankService
        {
            Task<IEnumerable<BankDto>> GetBanksAsync(int? parishId, int? bankId);
            Task<BankDto?> GetByIdAsync(int id);
            Task<IEnumerable<BankDto>> AddOrUpdateAsync(IEnumerable<BankDto> requests);
            Task<BankDto> UpdateAsync(BankDto bankDto);
            Task DeleteAsync(int id);
            Task<BankDto> AddAsync(BankDto bankDto);
        }
    }

}
