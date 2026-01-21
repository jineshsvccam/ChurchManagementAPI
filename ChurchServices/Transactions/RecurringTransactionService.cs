using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Transactions
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly IRecurringTransactionRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecurringTransactionService> _logger;

        public RecurringTransactionService(
            IRecurringTransactionRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<RecurringTransactionService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<RecurringTransactionDto>> GetAllAsync(int? parishId)
        {
            _logger.LogInformation("Fetching recurring transactions for ParishId: {ParishId}", parishId);
            var transactions = await _repository.GetAllAsync(parishId);
            return _mapper.Map<IEnumerable<RecurringTransactionDto>>(transactions);
        }

        public async Task<RecurringTransactionDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching recurring transaction by Id: {Id}", id);
            var transaction = await _repository.GetByIdAsync(id);
            if (transaction != null)
            {
                await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, transaction.ParishId);
            }
            return transaction != null ? _mapper.Map<RecurringTransactionDto>(transaction) : null;
        }

        public async Task<RecurringTransactionDto> AddAsync(RecurringTransactionDto dto)
        {
            var transaction = _mapper.Map<RecurringTransaction>(dto);
            var addedTransaction = await _repository.AddAsync(transaction);
            _logger.LogInformation("Added new recurring transaction with Id: {Id}", addedTransaction.RepeatedEntryId);
            return _mapper.Map<RecurringTransactionDto>(addedTransaction);
        }

        public async Task<RecurringTransactionDto> UpdateAsync(int id, RecurringTransactionDto dto)
        {
            var existingTransaction = await _repository.GetByIdAsync(id);
            if (existingTransaction == null)
                throw new KeyNotFoundException("Recurring Transaction not found");

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, existingTransaction.ParishId);

            _mapper.Map(dto, existingTransaction);
            var updatedTransaction = await _repository.UpdateAsync(existingTransaction);
            _logger.LogInformation("Updated recurring transaction with Id: {Id}", updatedTransaction.RepeatedEntryId);
            return _mapper.Map<RecurringTransactionDto>(updatedTransaction);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting recurring transaction with Id: {Id}", id);
            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<RecurringTransactionDto>> AddOrUpdateAsync(IEnumerable<RecurringTransactionDto> requests)
        {
            // Validate parish ownership for all DTOs in bulk request
            await ValidateBulkParishOwnershipAsync(requests);

            var processedEntries = new List<RecurringTransactionDto>();

            foreach (var request in requests)
            {
                if (string.Equals(request.Action, "INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    var created = await AddAsync(request);
                    processedEntries.Add(created);
                }
                else if (string.Equals(request.Action, "UPDATE", StringComparison.OrdinalIgnoreCase))
                {
                    var updated = await UpdateAsync(request.RepeatedEntryId, request);
                    processedEntries.Add(updated);
                }
                else
                {
                    _logger.LogWarning("Invalid action specified: {Action}", request.Action);
                    throw new ArgumentException($"Invalid action specified: {request.Action}");
                }
            }
            return processedEntries;
        }

        public async Task<int> DeleteByParishAndHeadAsync(int parishId, int headId)
        {
            _logger.LogInformation("Deleting recurring transactions for ParishId: {ParishId}, HeadId: {HeadId}", parishId, headId);
            return await _repository.DeleteByParishAndHeadAsync(parishId, headId);
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
