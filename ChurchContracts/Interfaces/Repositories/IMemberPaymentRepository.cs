using ChurchData;

namespace ChurchContracts
{
    namespace ChurchContracts
    {
        public interface IMemberPaymentRepository
        {
            Task<IEnumerable<MemberPayment>> GetByParishIdAsync(int parishId);
            Task<MemberPayment?> GetByIdAsync(int id);
            Task<MemberPayment> AddAsync(MemberPayment payment);
            Task<MemberPayment> UpdateAsync(MemberPayment payment);
            Task DeleteAsync(int id);
        }
    }
}
