using ChurchData;

namespace ChurchContracts
{
    namespace ChurchContracts
    {
        public interface IParishPaymentMethodRepository
        {
            Task<IEnumerable<ParishPaymentMethod>> GetByParishIdAsync(int parishId);
            Task<ParishPaymentMethod?> GetByIdAsync(int id);
            Task<ParishPaymentMethod> AddAsync(ParishPaymentMethod paymentMethod);
            Task<ParishPaymentMethod> UpdateAsync(ParishPaymentMethod paymentMethod);
            Task DeleteAsync(int id);
        }
    }
}
