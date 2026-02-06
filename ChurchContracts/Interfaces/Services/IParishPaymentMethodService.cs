using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    namespace ChurchContracts
    {
        public interface IParishPaymentMethodService
        {
            Task<IEnumerable<ParishPaymentMethodDto>> GetByParishIdAsync(int parishId);
            Task<ParishPaymentMethodDto?> GetByIdAsync(int id);
            Task<IEnumerable<ParishPaymentMethodDto>> AddOrUpdateAsync(IEnumerable<ParishPaymentMethodDto> requests);
            Task<ParishPaymentMethodDto> AddAsync(ParishPaymentMethodDto dto);
            Task<ParishPaymentMethodDto> UpdateAsync(ParishPaymentMethodDto dto);
            Task DeleteAsync(int id);
        }
    }
}
