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
        private readonly IReceiptIdGenerator _receiptIdGenerator;

        public MemberPaymentService(IMemberPaymentRepository repository,
                                   IMapper mapper,
                                   ILogger<MemberPaymentService> logger,
                                   IHttpContextAccessor httpContextAccessor,
                                   ApplicationDbContext context,
                                   IReceiptIdGenerator receiptIdGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _receiptIdGenerator = receiptIdGenerator;
        }

        public async Task<IEnumerable<MemberPaymentDto>> GetByParishIdAsync(int parishId)
        {
            _logger.LogInformation("Fetching payments for ParishId: {ParishId}", parishId);
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, parishId);
            var payments = await _repository.GetByParishIdAsync(parishId);
            return _mapper.Map<IEnumerable<MemberPaymentDto>>(payments);
        }

        public async Task<MemberPaymentDto?> GetByIdAsync(Guid id)
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

        public async Task<IEnumerable<MemberPaymentDto>> GetByReceiptIdAsync(string receiptId, int parishId)
        {
            _logger.LogInformation("Fetching payments for ReceiptId: {ReceiptId}, ParishId: {ParishId}", receiptId, parishId);
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, parishId);
            var payments = await _repository.GetByReceiptIdAsync(receiptId, parishId);
            return _mapper.Map<IEnumerable<MemberPaymentDto>>(payments);
        }

        public async Task<IEnumerable<MemberPaymentDto>> GetPendingByParishIdAsync(int parishId)
        {
            _logger.LogInformation("Fetching pending payments for ParishId: {ParishId}", parishId);
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, parishId);
            var payments = await _repository.GetPendingByParishIdAsync(parishId);
            return _mapper.Map<IEnumerable<MemberPaymentDto>>(payments);
        }

        public async Task<IEnumerable<MemberPaymentDto>> AddAsync(IEnumerable<MemberPaymentCreateDto> dtos)
        {
            var dtoList = dtos.ToList();
            if (!dtoList.Any())
            {
                throw new ArgumentException("At least one payment is required.");
            }

            foreach (var dto in dtoList)
            {
                ValidatePaymentDto(dto);
            }

            var (roleName, userParishId) = await UserHelper.GetCurrentUserRoleAndParishAsync(_httpContextAccessor, _context);

            if (!string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var dto in dtoList)
                {
                    if (userParishId == null || dto.ParishId != userParishId)
                    {
                        _logger.LogWarning("Unauthorized payment creation attempt for ParishId: {ParishId}", dto.ParishId);
                        throw new UnauthorizedAccessException("You are not authorized to create payments for another parish.");
                    }
                }
            }

            var userId = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);
            var receiptId = _receiptIdGenerator.Generate();
            var now = DateTimeOffset.UtcNow;

            var payments = dtoList.Select(dto =>
            {
                var payment = _mapper.Map<MemberPayment>(dto);
                payment.PaymentId = Guid.NewGuid();
                payment.ReceiptId = receiptId;
                payment.Status = "PENDING";
                payment.CreatedBy = userId;
                payment.CreatedAt = now;
                payment.ReceivedAt = now;
                return payment;
            }).ToList();

            await _repository.AddRangeAsync(payments);
            _logger.LogInformation("Added {Count} payments with ReceiptId: {ReceiptId}", payments.Count, receiptId);
            return _mapper.Map<IEnumerable<MemberPaymentDto>>(payments);
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

        public async Task<IEnumerable<MemberPaymentDto>> AddOrUpdateAsync(IEnumerable<MemberPaymentBulkItemDto> requests)
        {
            var requestList = requests.ToList();

            var (roleName, userParishId) = await UserHelper.GetCurrentUserRoleAndParishAsync(_httpContextAccessor, _context);

            if (!string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var request in requestList)
                {
                    if (userParishId == null || request.ParishId != userParishId)
                    {
                        _logger.LogWarning("Unauthorized bulk payment attempt for ParishId: {ParishId}", request.ParishId);
                        throw new UnauthorizedAccessException("You are not authorized to modify payments for another parish.");
                    }
                }
            }

            var inserts = requestList.Where(r => string.Equals(r.Action, "INSERT", StringComparison.OrdinalIgnoreCase)).ToList();
            var updates = requestList.Where(r => string.Equals(r.Action, "UPDATE", StringComparison.OrdinalIgnoreCase)).ToList();
            var invalid = requestList.Where(r => !string.Equals(r.Action, "INSERT", StringComparison.OrdinalIgnoreCase)
                                               && !string.Equals(r.Action, "UPDATE", StringComparison.OrdinalIgnoreCase)).ToList();

            if (invalid.Any())
            {
                _logger.LogWarning("Invalid action specified: {Action}", invalid.First().Action);
                throw new ArgumentException($"Invalid action specified: {invalid.First().Action}");
            }

            var results = new List<MemberPaymentDto>();

            if (inserts.Any())
            {
                var createDtos = inserts.Select(r => new MemberPaymentCreateDto
                {
                    ParishId = r.ParishId,
                    FamilyId = r.FamilyId,
                    MemberId = r.MemberId,
                    HeadId = r.HeadId,
                    PaymentMethodId = r.PaymentMethodId,
                    BankId = r.BankId,
                    Amount = r.Amount,
                    PaymentMode = r.PaymentMode,
                    UtrNumber = r.UtrNumber,
                    ReferenceNumber = r.ReferenceNumber,
                    PaymentDate = r.PaymentDate,
                    Remarks = r.Remarks
                });
                var added = await AddAsync(createDtos);
                results.AddRange(added);
            }

            foreach (var request in updates)
            {
                var updateDto = new MemberPaymentUpdateDto
                {
                    PaymentId = request.PaymentId,
                    Amount = request.Amount,
                    PaymentMode = request.PaymentMode,
                    UtrNumber = request.UtrNumber,
                    ReferenceNumber = request.ReferenceNumber,
                    PaymentDate = request.PaymentDate,
                    Remarks = request.Remarks
                };
                var updated = await UpdateAsync(updateDto);
                results.Add(updated);
            }

            return results;
        }

        public async Task<IEnumerable<MemberPaymentDto>> ApproveOrRejectAsync(MemberPaymentApprovalDto dto)
        {
            var validStatuses = new[] { "APPROVED", "REJECTED" };
            if (!validStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Status must be one of: APPROVED, REJECTED");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, dto.ParishId);
            var approvedBy = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);
            var status = dto.Status.ToUpperInvariant();

            if (dto.PaymentId.HasValue && dto.PaymentId.Value != Guid.Empty)
            {
                _logger.LogInformation("Processing {Status} for PaymentId: {PaymentId}", status, dto.PaymentId.Value);
                await _repository.UpdatePaymentStatusAsync(dto.PaymentId.Value, status, approvedBy, dto.Remarks);
            }
            else
            {
                _logger.LogInformation("Processing {Status} for ReceiptId: {ReceiptId}, ParishId: {ParishId}", status, dto.ReceiptId, dto.ParishId);
                await _repository.UpdateReceiptStatusAsync(dto.ReceiptId, dto.ParishId, status, approvedBy, dto.Remarks);
            }

            var updatedPayments = await _repository.GetByReceiptIdAsync(dto.ReceiptId, dto.ParishId);
            _logger.LogInformation("{Status} payments for ReceiptId: {ReceiptId}, count: {Count}", status, dto.ReceiptId, updatedPayments.Count());
            return _mapper.Map<IEnumerable<MemberPaymentDto>>(updatedPayments);
        }

        public async Task DeleteAsync(Guid id)
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

            var validModes = new[] { "UPI", "CASH", "BANK", "GATEWAY" };
            if (!validModes.Contains(dto.PaymentMode, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("PaymentMode must be one of: UPI, CASH, BANK, GATEWAY");
            }

            if (string.Equals(dto.PaymentMode, "UPI", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(dto.UtrNumber))
            {
                throw new ArgumentException("UtrNumber is mandatory when PaymentMode is UPI");
            }
        }

        private void ValidateUpdateDto(MemberPaymentUpdateDto dto)
        {
            if (dto.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0");
            }

            var validModes = new[] { "UPI", "CASH", "BANK", "GATEWAY" };
            if (!validModes.Contains(dto.PaymentMode, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("PaymentMode must be one of: UPI, CASH, BANK, GATEWAY");
            }

            if (string.Equals(dto.PaymentMode, "UPI", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(dto.UtrNumber))
            {
                throw new ArgumentException("UtrNumber is mandatory when PaymentMode is UPI");
            }
        }
    }
}
