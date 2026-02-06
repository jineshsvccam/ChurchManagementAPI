using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    namespace ChurchContracts
    {
        public interface IMemberPaymentService
        {
            Task<IEnumerable<MemberPaymentDto>> GetByParishIdAsync(int parishId);
            Task<MemberPaymentDto?> GetByIdAsync(int id);
            Task<MemberPaymentDto> AddAsync(MemberPaymentCreateDto dto);
            Task<MemberPaymentDto> UpdateAsync(MemberPaymentUpdateDto dto);
            Task DeleteAsync(int id);
        }
    }
}
