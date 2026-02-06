using ChurchCommon.Utils;
using ChurchContracts.ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories.Payments
{
    public class MemberPaymentRepository : IMemberPaymentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MemberPaymentRepository(ApplicationDbContext context,
                                      IHttpContextAccessor httpContextAccessor,
                                      LogsHelper logsHelper)
        {
            _context = context;
            _logsHelper = logsHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<MemberPayment>> GetByParishIdAsync(int parishId)
        {
            return await _context.MemberPayments
                .Where(m => m.ParishId == parishId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<MemberPayment?> GetByIdAsync(int id)
        {
            return await _context.MemberPayments.FindAsync(id);
        }

        public async Task<MemberPayment> AddAsync(MemberPayment payment)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, payment.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            await _context.MemberPayments.AddAsync(payment);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("member_payments", payment.PaymentId, "INSERT", userId, null, Extensions.Serialize(payment));
            return payment;
        }

        public async Task<MemberPayment> UpdateAsync(MemberPayment payment)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, payment.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existingPayment = await _context.MemberPayments.FindAsync(payment.PaymentId);
            if (existingPayment == null)
            {
                throw new KeyNotFoundException("Payment not found");
            }

            var oldValues = existingPayment.Clone();
            _context.Entry(existingPayment).CurrentValues.SetValues(payment);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("member_payments", payment.PaymentId, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(payment));
            return payment;
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var payment = await _context.MemberPayments.FindAsync(id);
            if (payment == null)
            {
                throw new KeyNotFoundException("Payment not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, payment.ParishId);

            _context.MemberPayments.Remove(payment);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("member_payments", payment.PaymentId, "DELETE", userId, Extensions.Serialize(payment), null);
        }
    }
}
