using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Settings
{
    public class TransactionHeadService : ITransactionHeadService
    {
        private readonly ITransactionHeadRepository _transactionHeadRepository;
        private readonly ILogger<TransactionHeadService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public TransactionHeadService(ITransactionHeadRepository transactionHeadRepository,
                                      ILogger<TransactionHeadService> logger,
                                      IMapper mapper,
                                      IHttpContextAccessor httpContextAccessor,
                                      ApplicationDbContext context)
        {
            _transactionHeadRepository = transactionHeadRepository ?? throw new ArgumentNullException(nameof(transactionHeadRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<TransactionHeadDto>> GetTransactionHeadsAsync(int? parishId, int? headId)
        {
            _logger.LogInformation("Fetching transaction heads for ParishId: {ParishId}, HeadId: {HeadId}", parishId, headId);
            var result = await _transactionHeadRepository.GetTransactionHeadsAsync(parishId, headId);
            return _mapper.Map<IEnumerable<TransactionHeadDto>>(result);
        }

        public async Task<TransactionHeadDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching transaction head by Id: {Id}", id);
            var result = await _transactionHeadRepository.GetByIdAsync(id);
            if (result != null)
            {
                await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, result.ParishId);
            }
            return _mapper.Map<TransactionHeadDto?>(result);
        }

        public async Task<IEnumerable<TransactionHeadDto>> AddOrUpdateAsync(IEnumerable<TransactionHeadDto> requests)
        {
            // Validate parish ownership for all DTOs in bulk request
            await ValidateBulkParishOwnershipAsync(requests);

            var processedHeads = new List<TransactionHeadDto>();

            foreach (var request in requests)
            {
                if (string.Equals(request.Action, "INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    var createdHead = await AddAsync(request);
                    processedHeads.Add(createdHead);
                }
                else if (string.Equals(request.Action, "UPDATE", StringComparison.OrdinalIgnoreCase))
                {
                    var updatedHead = await UpdateAsync(request);
                    processedHeads.Add(updatedHead);
                }
                else
                {
                    _logger.LogWarning("Invalid action specified: {Action}", request.Action);
                    throw new ArgumentException($"Invalid action specified: {request.Action}");
                }
            }
            return processedHeads;
        }

        public async Task<TransactionHeadDto> AddAsync(TransactionHeadDto transactionHeadDto)
        {
            var entity = _mapper.Map<TransactionHead>(transactionHeadDto);
            var addedEntity = await _transactionHeadRepository.AddAsync(entity);
            _logger.LogInformation("Added new transaction head with Id: {HeadId}", addedEntity.HeadId);
            return _mapper.Map<TransactionHeadDto>(addedEntity);
        }

        public async Task<TransactionHeadDto> UpdateAsync(TransactionHeadDto transactionHeadDto)
        {
            var existingEntity = await _transactionHeadRepository.GetByIdAsync(transactionHeadDto.HeadId);
            if (existingEntity == null)
            {
                throw new KeyNotFoundException("Transaction head not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, existingEntity.ParishId);

            var entity = _mapper.Map<TransactionHead>(transactionHeadDto);
            var updatedEntity = await _transactionHeadRepository.UpdateAsync(entity);
            _logger.LogInformation("Updated transaction head with Id: {HeadId}", updatedEntity.HeadId);
            return _mapper.Map<TransactionHeadDto>(updatedEntity);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting transaction head with Id: {Id}", id);
            await _transactionHeadRepository.DeleteAsync(id);
        }

        private async Task ValidateBulkParishOwnershipAsync<TDto>(IEnumerable<TDto> requests) where TDto : class
        {
            var (roleName, userParishId) = await UserHelper.GetCurrentUserRoleAndParishAsync(_httpContextAccessor, _context);

            // Admin users can modify any parish data
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
