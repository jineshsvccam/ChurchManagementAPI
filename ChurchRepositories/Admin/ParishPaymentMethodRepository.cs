using ChurchCommon.Utils;
using ChurchContracts.ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories.Admin
{
    public class ParishPaymentMethodRepository : IParishPaymentMethodRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ParishPaymentMethodRepository(ApplicationDbContext context,
                                           IHttpContextAccessor httpContextAccessor,
                                           LogsHelper logsHelper)
        {
            _context = context;
            _logsHelper = logsHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ParishPaymentMethod>> GetByParishIdAsync(int parishId)
        {
            return await _context.ParishPaymentMethods
                .Where(p => p.ParishId == parishId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ParishPaymentMethod?> GetByIdAsync(int id)
        {
            return await _context.ParishPaymentMethods.FindAsync(id);
        }

        public async Task<ParishPaymentMethod> AddAsync(ParishPaymentMethod paymentMethod)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, paymentMethod.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            await _context.ParishPaymentMethods.AddAsync(paymentMethod);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("parish_payment_methods", paymentMethod.PaymentMethodId, "INSERT", userId, null, Extensions.Serialize(paymentMethod));
            return paymentMethod;
        }

        public async Task<ParishPaymentMethod> UpdateAsync(ParishPaymentMethod paymentMethod)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, paymentMethod.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existingMethod = await _context.ParishPaymentMethods.FindAsync(paymentMethod.PaymentMethodId);
            if (existingMethod == null)
            {
                throw new KeyNotFoundException("Payment method not found");
            }

            var oldValues = existingMethod.Clone();
            _context.Entry(existingMethod).CurrentValues.SetValues(paymentMethod);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("parish_payment_methods", paymentMethod.PaymentMethodId, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(paymentMethod));
            return paymentMethod;
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var paymentMethod = await _context.ParishPaymentMethods.FindAsync(id);
            if (paymentMethod == null)
            {
                throw new KeyNotFoundException("Payment method not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, paymentMethod.ParishId);

            _context.ParishPaymentMethods.Remove(paymentMethod);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("parish_payment_methods", paymentMethod.PaymentMethodId, "DELETE", userId, Extensions.Serialize(paymentMethod), null);
        }
    }
}
