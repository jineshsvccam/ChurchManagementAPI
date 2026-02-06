using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    namespace ChurchContracts
    {
        public interface IMemberPaymentService
        {
            Task<IEnumerable<MemberPaymentDto>> GetByParishIdAsync(int parishId);
            Task<MemberPaymentDto?> GetByIdAsync(Guid id);
            Task<IEnumerable<MemberPaymentDto>> GetByReceiptIdAsync(string receiptId, int parishId);
            Task<IEnumerable<MemberPaymentDto>> GetPendingByParishIdAsync(int parishId);
            Task<IEnumerable<MemberPaymentDto>> AddAsync(IEnumerable<MemberPaymentCreateDto> dtos);
            Task<MemberPaymentDto> UpdateAsync(MemberPaymentUpdateDto dto);
            Task<IEnumerable<MemberPaymentDto>> AddOrUpdateAsync(IEnumerable<MemberPaymentBulkItemDto> requests);
            Task<IEnumerable<MemberPaymentDto>> ApproveOrRejectAsync(MemberPaymentApprovalDto dto);
            Task DeleteAsync(Guid id);
        }
    }
}
