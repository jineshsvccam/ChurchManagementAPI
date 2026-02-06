using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts.ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Payments
{
    public class MemberPaymentService : IMemberPaymentService
    {
        private readonly IMemberPaymentRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<MemberPaymentService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public MemberPaymentService(IMemberPaymentRepository repository,
                                   IMapper mapper,
                                   ILogger<MemberPaymentService> logger,
                                   IHttpContextAccessor httpContextAccessor,
                                   ApplicationDbContext context)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<IEnumerable<MemberPaymentDto>> GetByParishIdAsync(int parishId)
        {
            _logger.LogInformation("Fetching payments for ParishId: {ParishId}", parishId);
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, parishId);
            var payments = await _repository.GetByParishIdAsync(parishId);
            return _mapper.Map<IEnumerable<MemberPaymentDto>>(payments);
        }

        public async Task<MemberPaymentDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching payment by Id: {Id}", id);
            var payment = await _repository.GetByIdAsync(id);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found with Id: {Id}", id);
                return null;
            }
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, payment.ParishId);
            return _mapper.Map<MemberPaymentDto>(payment);
        }

        public async Task<MemberPaymentDto> AddAsync(MemberPaymentCreateDto dto)
        {
            ValidatePaymentDto(dto);
            
            var (roleName, userParishId) = await UserHelper.GetCurrentUserRoleAndParishAsync(_httpContextAccessor, _context);
            
            if (!string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (userParishId == null || dto.ParishId != userParishId)
                {
                    _logger.LogWarning("Unauthorized payment creation attempt for ParishId: {ParishId}", dto.ParishId);
                    throw new UnauthorizedAccessException("You are not authorized to create payments for another parish.");
                }
            }

            var userId = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);
            
            var payment = _mapper.Map<MemberPayment>(dto);
            payment.CreatedBy = userId;
            payment.CreatedAt = DateTime.UtcNow;
            payment.ReceivedAt = DateTime.UtcNow;

            var added = await _repository.AddAsync(payment);
            _logger.LogInformation("Added new payment with Id: {PaymentId}", added.PaymentId);
            return _mapper.Map<MemberPaymentDto>(added);
        }

        public async Task<MemberPaymentDto> UpdateAsync(MemberPaymentUpdateDto dto)
        {
            ValidateUpdateDto(dto);

            var existingPayment = await _repository.GetByIdAsync(dto.PaymentId);
            if (existingPayment == null)
            {
                _logger.LogWarning("Payment not found for update with Id: {PaymentId}", dto.PaymentId);
                throw new KeyNotFoundException("Payment not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, existingPayment.ParishId);

            existingPayment.Amount = dto.Amount;
            existingPayment.PaymentMode = dto.PaymentMode;
            existingPayment.UtrNumber = dto.UtrNumber;
            existingPayment.ReferenceNumber = dto.ReferenceNumber;
            existingPayment.PaymentDate = dto.PaymentDate;
            existingPayment.Remarks = dto.Remarks;

            var updated = await _repository.UpdateAsync(existingPayment);
            _logger.LogInformation("Updated payment with Id: {PaymentId}", updated.PaymentId);
            return _mapper.Map<MemberPaymentDto>(updated);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting payment with Id: {Id}", id);
            var payment = await _repository.GetByIdAsync(id);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found for deletion with Id: {Id}", id);
                throw new KeyNotFoundException("Payment not found");
            }
            await _repository.DeleteAsync(id);
        }

        private void ValidatePaymentDto(MemberPaymentCreateDto dto)
        {
            if (dto.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0");
            }

            if (string.Equals(dto.PaymentMode, "UPI", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(dto.UtrNumber))
            {
                throw new ArgumentException("UtrNumber is mandatory when PaymentMode is UPI");
            }

            var validModes = new[] { "UPI", "CASH", "BANK", "GATEWAY" };
            if (!validModes.Contains(dto.PaymentMode, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("PaymentMode must be one of: UPI, CASH, BANK, GATEWAY");
            }
        }

        private void ValidateUpdateDto(MemberPaymentUpdateDto dto)
        {
            if (dto.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0");
            }

            if (string.Equals(dto.PaymentMode, "UPI", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(dto.UtrNumber))
            {
                throw new ArgumentException("UtrNumber is mandatory when PaymentMode is UPI");
            }

            var validModes = new[] { "UPI", "CASH", "BANK", "GATEWAY" };
            if (!validModes.Contains(dto.PaymentMode, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("PaymentMode must be one of: UPI, CASH, BANK, GATEWAY");
            }
        }
    }
}
