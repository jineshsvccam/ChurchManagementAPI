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
                .OrderByDescending(m => m.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<MemberPayment?> GetByIdAsync(Guid id)
        {
            return await _context.MemberPayments.FindAsync(id);
        }

        public async Task<IEnumerable<MemberPayment>> GetByReceiptIdAsync(string receiptId, int parishId)
        {
            return await _context.MemberPayments
                .Where(m => m.ReceiptId == receiptId && m.ParishId == parishId)
                .OrderByDescending(m => m.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<MemberPayment>> GetPendingByParishIdAsync(int parishId)
        {
            return await _context.MemberPayments
                .Where(m => m.ParishId == parishId && m.Status == "PENDING")
                .OrderByDescending(m => m.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<MemberPayment> payments)
        {
            var paymentList = payments.ToList();
            if (!paymentList.Any()) return;

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, paymentList.First().ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            await _context.MemberPayments.AddRangeAsync(paymentList);
            await _context.SaveChangesAsync();

            foreach (var payment in paymentList)
            {
                await _logsHelper.LogChangeAsync("member_payments", 0, "INSERT", userId, null, Extensions.Serialize(payment));
            }
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
            await _logsHelper.LogChangeAsync("member_payments", 0, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(payment));
            return payment;
        }

        public async Task UpdateReceiptStatusAsync(string receiptId, int parishId, string status, Guid approvedBy, string? remarks)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, parishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var payments = await _context.MemberPayments
                .Where(m => m.ReceiptId == receiptId && m.ParishId == parishId && m.Status == "PENDING")
                .ToListAsync();

            if (!payments.Any())
            {
                throw new KeyNotFoundException("No pending payments found for this receipt.");
            }

            var now = DateTimeOffset.UtcNow;
            foreach (var payment in payments)
            {
                var oldValues = payment.Clone();
                payment.Status = status;
                payment.ApprovedAt = now;
                payment.ApprovedBy = approvedBy;
                if (remarks != null)
                {
                    payment.Remarks = remarks;
                }
                await _logsHelper.LogChangeAsync("member_payments", 0, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(payment));
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdatePaymentStatusAsync(Guid paymentId, string status, Guid approvedBy, string? remarks)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var payment = await _context.MemberPayments.FindAsync(paymentId);
            if (payment == null)
            {
                throw new KeyNotFoundException("Payment not found.");
            }

            if (payment.Status != "PENDING")
            {
                throw new InvalidOperationException($"Payment is already {payment.Status}.");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, payment.ParishId);

            var oldValues = payment.Clone();
            payment.Status = status;
            payment.ApprovedAt = DateTimeOffset.UtcNow;
            payment.ApprovedBy = approvedBy;
            if (remarks != null)
            {
                payment.Remarks = remarks;
            }

            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("member_payments", 0, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(payment));
        }

        public async Task DeleteAsync(Guid id)
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
            await _logsHelper.LogChangeAsync("member_payments", 0, "DELETE", userId, Extensions.Serialize(payment), null);
        }
    }
}
