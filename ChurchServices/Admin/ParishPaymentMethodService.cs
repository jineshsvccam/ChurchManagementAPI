using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts.ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Admin
{
    public class ParishPaymentMethodService : IParishPaymentMethodService
    {
        private readonly IParishPaymentMethodRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ParishPaymentMethodService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public ParishPaymentMethodService(IParishPaymentMethodRepository repository,
                                        IMapper mapper,
                                        ILogger<ParishPaymentMethodService> logger,
                                        IHttpContextAccessor httpContextAccessor,
                                        ApplicationDbContext context)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<IEnumerable<ParishPaymentMethodDto>> GetByParishIdAsync(int parishId)
        {
            _logger.LogInformation("Fetching payment methods for ParishId: {ParishId}", parishId);
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, parishId);
            var methods = await _repository.GetByParishIdAsync(parishId);
            return _mapper.Map<IEnumerable<ParishPaymentMethodDto>>(methods);
        }

        public async Task<ParishPaymentMethodDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching payment method by Id: {Id}", id);
            var method = await _repository.GetByIdAsync(id);
            if (method == null)
            {
                _logger.LogWarning("Payment method not found with Id: {Id}", id);
                return null;
            }
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, method.ParishId);
            return _mapper.Map<ParishPaymentMethodDto>(method);
        }

        public async Task<IEnumerable<ParishPaymentMethodDto>> AddOrUpdateAsync(IEnumerable<ParishPaymentMethodDto> requests)
        {
            await ValidateBulkParishOwnershipAsync(requests);

            var results = new List<ParishPaymentMethodDto>();
            foreach (var request in requests)
            {
                if (string.Equals(request.Action, "INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    ValidatePaymentMethodDto(request);
                    var added = await AddAsync(request);
                    results.Add(added);
                }
                else if (string.Equals(request.Action, "UPDATE", StringComparison.OrdinalIgnoreCase))
                {
                    ValidatePaymentMethodDto(request);
                    var updated = await UpdateAsync(request);
                    results.Add(updated);
                }
                else
                {
                    _logger.LogWarning("Invalid action specified: {Action}", request.Action);
                    throw new ArgumentException($"Invalid action specified: {request.Action}");
                }
            }
            return results;
        }

        public async Task<ParishPaymentMethodDto> AddAsync(ParishPaymentMethodDto dto)
        {
            ValidatePaymentMethodDto(dto);
            var entity = _mapper.Map<ParishPaymentMethod>(dto);
            var added = await _repository.AddAsync(entity);
            _logger.LogInformation("Added new payment method with Id: {PaymentMethodId}", added.PaymentMethodId);
            return _mapper.Map<ParishPaymentMethodDto>(added);
        }

        public async Task<ParishPaymentMethodDto> UpdateAsync(ParishPaymentMethodDto dto)
        {
            ValidatePaymentMethodDto(dto);
            var existingMethod = await _repository.GetByIdAsync(dto.PaymentMethodId);
            if (existingMethod == null)
            {
                _logger.LogWarning("Payment method not found for update with Id: {PaymentMethodId}", dto.PaymentMethodId);
                throw new KeyNotFoundException("Payment method not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, existingMethod.ParishId);

            var entity = _mapper.Map<ParishPaymentMethod>(dto);
            var updated = await _repository.UpdateAsync(entity);
            _logger.LogInformation("Updated payment method with Id: {PaymentMethodId}", updated.PaymentMethodId);
            return _mapper.Map<ParishPaymentMethodDto>(updated);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting payment method with Id: {Id}", id);
            var method = await _repository.GetByIdAsync(id);
            if (method == null)
            {
                _logger.LogWarning("Payment method not found for deletion with Id: {Id}", id);
                throw new KeyNotFoundException("Payment method not found");
            }
            await _repository.DeleteAsync(id);
        }

        private void ValidatePaymentMethodDto(ParishPaymentMethodDto dto)
        {
            if (string.Equals(dto.MethodType, "UPI", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(dto.UpiId))
            {
                throw new ArgumentException("UpiId is mandatory when MethodType is UPI");
            }

            if (string.Equals(dto.MethodType, "BANK", StringComparison.OrdinalIgnoreCase) && !dto.BankId.HasValue)
            {
                throw new ArgumentException("BankId is mandatory when MethodType is BANK");
            }
        }

        private async Task ValidateBulkParishOwnershipAsync<TDto>(IEnumerable<TDto> requests) where TDto : class
        {
            var (roleName, userParishId) = await UserHelper.GetCurrentUserRoleAndParishAsync(_httpContextAccessor, _context);

            if (string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (var request in requests)
            {
                if (request is ChurchDTOs.DTOs.Utils.IParishEntity parishEntity)
                {
                    if (userParishId == null || parishEntity.ParishId != userParishId)
                    {
                        throw new UnauthorizedAccessException("You are not authorized to modify data from another parish.");
                    }
                }
            }
        }
    }
}
