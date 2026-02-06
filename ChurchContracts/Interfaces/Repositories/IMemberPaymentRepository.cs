using ChurchData;

namespace ChurchContracts
{
    namespace ChurchContracts
    {
        public interface IMemberPaymentRepository
        {
            Task<IEnumerable<MemberPayment>> GetByParishIdAsync(int parishId);
            Task<MemberPayment?> GetByIdAsync(Guid id);
            Task<IEnumerable<MemberPayment>> GetByReceiptIdAsync(string receiptId, int parishId);
            Task<IEnumerable<MemberPayment>> GetPendingByParishIdAsync(int parishId);
            Task AddRangeAsync(IEnumerable<MemberPayment> payments);
            Task<MemberPayment> UpdateAsync(MemberPayment payment);
            Task UpdateReceiptStatusAsync(string receiptId, int parishId, string status, Guid approvedBy, string? remarks);
            Task UpdatePaymentStatusAsync(Guid paymentId, string status, Guid approvedBy, string? remarks);
            Task DeleteAsync(Guid id);
        }
    }
}
