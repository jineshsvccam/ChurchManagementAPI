using ChurchContracts;
using ChurchContracts.Utils;
using ChurchDTOs.DTOs.Entities;
using ChurchData;
using Microsoft.Extensions.Logging;
using AutoMapper;
using ChurchCommon.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace ChurchServices
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
            var (_, userParishId, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);

            if (parishId.HasValue && parishId != userParishId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view transactions for this parish.");
            }

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
            var (_, userParishId, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);

            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction with id {id} not found.", id);
                return null;
            }

            if (transaction.ParishId != userParishId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this transaction.");
            }

            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<IEnumerable<TransactionDto>> AddOrUpdateAsync(IEnumerable<TransactionDto> requests)
        {
            var (_, userParishId, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);

            if (requests.Any(r => r.ParishId != userParishId))
            {
                throw new UnauthorizedAccessException("One or more transactions belong to a different parish. Operation not allowed.");
            }

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
            var (_, userParishId, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);

            if (transactionDto.ParishId != userParishId)
            {
                throw new InvalidOperationException("You do not have permission to insert data for this parish.");
            }

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
            var (_, userParishId, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);

            var existingTransaction = await _transactionRepository.GetByIdAsync(transactionDto.TransactionId);
            if (existingTransaction == null)
            {
                throw new InvalidOperationException("Transaction not found.");
            }

            if (existingTransaction.ParishId != userParishId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this transaction.");
            }

            var financialYear = await _financialYearRepository.GetFinancialYearByDateAsync(transactionDto.ParishId, transactionDto.TrDate);
            if (financialYear == null || financialYear.IsLocked)
            {
                throw new InvalidOperationException("Transactions for this financial year are locked and cannot be modified.");
            }

            var transaction = _mapper.Map<Transaction>(transactionDto);
            transaction.UpdatedBy = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);
            transaction.UpdatedAt = DateTime.UtcNow;

            var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
            return _mapper.Map<TransactionDto>(updatedTransaction);
        }

        public async Task DeleteAsync(int id)
        {
            var (_, userParishId, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);

            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                throw new InvalidOperationException("Transaction not found.");
            }

            if (transaction.ParishId != userParishId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this transaction.");
            }

            var financialYear = await _financialYearRepository.GetFinancialYearByDateAsync(transaction.ParishId, transaction.TrDate);
            if (financialYear == null || financialYear.IsLocked)
            {
                throw new InvalidOperationException("Transactions for this financial year are locked and cannot be modified.");
            }

            await _transactionRepository.DeleteAsync(id);
            _logger.LogInformation("Transaction with id {id} deleted.", id);
        }
    }
}
