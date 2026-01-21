using ChurchContracts;
using ChurchContracts.Utils;
using ChurchDTOs.DTOs.Entities;
using ChurchData;
using Microsoft.Extensions.Logging;
using AutoMapper;
using ChurchCommon.Utils;
using Microsoft.AspNetCore.Http;

namespace ChurchServices.Transactions
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IFinancialYearRepository _financialYearRepository;
        private readonly ILogger<TransactionService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IFinancialYearRepository financialYearRepository,
            ILogger<TransactionService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context)
        {
            _transactionRepository = transactionRepository;
            _financialYearRepository = financialYearRepository;
            _logger = logger;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<PagedResult<TransactionDto>> GetTransactionsAsync(
            int? parishId, int? familyId, int? transactionId, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching transactions for parish {parishId}", parishId);
            var result = await _transactionRepository.GetTransactionsAsync(parishId, familyId, transactionId, startDate, endDate, pageNumber, pageSize);

            return new PagedResult<TransactionDto>
            {
                Items = _mapper.Map<List<TransactionDto>>(result.Items),
                TotalCount = result.TotalCount
            };
        }

        public async Task<TransactionDto?> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction with id {id} not found.", id);
                return null;
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, transaction.ParishId);

            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<IEnumerable<TransactionDto>> AddOrUpdateAsync(IEnumerable<TransactionDto> requests)
        {
            // Validate parish ownership for all DTOs in bulk request
            await ValidateBulkParishOwnershipAsync(requests);

            var transactions = new List<TransactionDto>();

            foreach (var request in requests)
            {
                transactions.Add(request.Action switch
                {
                    "INSERT" => await AddAsync(request),
                    "UPDATE" => await UpdateAsync(request),
                    _ => throw new ArgumentException("Invalid action specified")
                });
            }

            return transactions;
        }

        public async Task<TransactionDto> AddAsync(TransactionDto transactionDto)
        {
            var financialYear = await _financialYearRepository.GetFinancialYearByDateAsync(transactionDto.ParishId, transactionDto.TrDate);
            if (financialYear == null || financialYear.IsLocked)
            {
                throw new InvalidOperationException("Transactions for this financial year are locked and cannot be modified.");
            }

            var transaction = _mapper.Map<Transaction>(transactionDto);
            transaction.CreatedBy = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);

            var addedTransaction = await _transactionRepository.AddAsync(transaction);
            return _mapper.Map<TransactionDto>(addedTransaction);
        }

        public async Task<TransactionDto> UpdateAsync(TransactionDto transactionDto)
        {
            var existingTransaction = await _transactionRepository.GetByIdAsync(transactionDto.TransactionId);
            if (existingTransaction == null)
            {
                throw new InvalidOperationException("Transaction not found.");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, existingTransaction.ParishId);

            var financialYear = await _financialYearRepository.GetFinancialYearByDateAsync(transactionDto.ParishId, transactionDto.TrDate);
            if (financialYear == null || financialYear.IsLocked)
            {
                throw new InvalidOperationException("Transactions for this financial year are locked and cannot be modified.");
            }

            var originalCreatedBy = existingTransaction.CreatedBy;

            var transaction = _mapper.Map<Transaction>(transactionDto);
            transaction.UpdatedBy = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.CreatedBy = originalCreatedBy;

            var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
            return _mapper.Map<TransactionDto>(updatedTransaction);
        }

        public async Task DeleteAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                throw new InvalidOperationException("Transaction not found.");
            }

            var financialYear = await _financialYearRepository.GetFinancialYearByDateAsync(transaction.ParishId, transaction.TrDate);
            if (financialYear == null || financialYear.IsLocked)
            {
                throw new InvalidOperationException("Transactions for this financial year are locked and cannot be modified.");
            }

            await _transactionRepository.DeleteAsync(id);
            _logger.LogInformation("Transaction with id {id} deleted.", id);
        }

        public async Task DeleteAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                throw new ArgumentException("Transaction IDs array cannot be null or empty.");
            }

            if (ids.Length > 10)
            {
                throw new ArgumentException("Cannot delete more than 10 transactions at once.");
            }

            var transactions = await _transactionRepository.GetByIdsAsync(ids);

            if (transactions.Count != ids.Length)
            {
                var foundIds = transactions.Select(t => t.TransactionId).ToHashSet();
                var missingIds = ids.Where(id => !foundIds.Contains(id));
                throw new InvalidOperationException($"Some transactions were not found: {string.Join(", ", missingIds)}");
            }

            // Get user's parish for financial year validation
            var (_, userParishId, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);

            var transactionDates = transactions.Select(t => t.TrDate).Distinct().ToList();
            var financialYears = await _financialYearRepository.GetFinancialYearsByDatesAsync((int)userParishId, transactionDates);

            foreach (var transaction in transactions)
            {
                var financialYear = financialYears.FirstOrDefault(fy =>
                    fy.ParishId == transaction.ParishId &&
                    fy.StartDate <= transaction.TrDate &&
                    fy.EndDate >= transaction.TrDate);

                if (financialYear == null || financialYear.IsLocked)
                {
                    throw new InvalidOperationException(
                        $"Transaction {transaction.TransactionId} cannot be deleted as its financial year is locked.");
                }
            }

            await _transactionRepository.DeleteMultipleAsync(ids);

            _logger.LogInformation("Transactions with ids {ids} deleted.", string.Join(", ", ids));
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
