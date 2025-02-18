using ChurchContracts;
using ChurchContracts.Utils;
using ChurchDTOs.DTOs.Entities;
using ChurchData;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace ChurchServices
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IFinancialYearRepository _financialYearRepository;
        private readonly ILogger<TransactionService> _logger;
        private readonly IMapper _mapper;

        public TransactionService(ITransactionRepository transactionRepository, IFinancialYearRepository financialYearRepository, ILogger<TransactionService> logger, IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _financialYearRepository = financialYearRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<PagedResult<TransactionDto>> GetTransactionsAsync(int? parishId, int? familyId, int? transactionId, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Fetching transactions with filters: parishId={parishId}, familyId={familyId}, transactionId={transactionId}, startDate={startDate}, endDate={endDate}", parishId, familyId, transactionId, startDate, endDate);
            var result = await _transactionRepository.GetTransactionsAsync(parishId, familyId, transactionId, startDate, endDate, pageNumber, pageSize);
            var transactionDtos = _mapper.Map<List<TransactionDto>>(result.Items);
            return new PagedResult<TransactionDto>
            {
                Items = transactionDtos,
                TotalCount = result.TotalCount
            };
        }

        public async Task<TransactionDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching transaction by id: {id}", id);
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction with id {id} not found.", id);
                return null;
            }
            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<IEnumerable<TransactionDto>> AddOrUpdateAsync(IEnumerable<TransactionDto> requests)
        {
            var createdTransactions = new List<TransactionDto>();

            foreach (var request in requests)
            {
                if (request.Action == "INSERT")
                {
                    var createdTransaction = await AddAsync(request);
                    createdTransactions.Add(createdTransaction);
                }
                else if (request.Action == "UPDATE")
                {
                    var updatedTransaction = await UpdateAsync(request);
                    createdTransactions.Add(updatedTransaction);
                }
                else
                {
                    _logger.LogError("Invalid action specified for transaction: {action}", request.Action);
                    throw new ArgumentException("Invalid action specified");
                }
            }

            return createdTransactions;
        }

        public async Task<TransactionDto> AddAsync(TransactionDto transactionDto)
        {
            _logger.LogInformation("Adding new transaction: {@transactionDto}", transactionDto);
            var financialYear = await _financialYearRepository.GetFinancialYearByDateAsync(transactionDto.ParishId, transactionDto.TrDate);

            if (financialYear == null || financialYear.IsLocked)
            {
                _logger.LogError("Transactions for the financial year are locked. Cannot add transaction.");
                throw new InvalidOperationException("Transactions for this financial year are locked and cannot be modified.");
            }

            var transaction = _mapper.Map<Transaction>(transactionDto);
            var addedTransaction = await _transactionRepository.AddAsync(transaction);
            return _mapper.Map<TransactionDto>(addedTransaction);
        }

        public async Task<TransactionDto> UpdateAsync(TransactionDto transactionDto)
        {
            _logger.LogInformation("Updating transaction: {@transactionDto}", transactionDto);
            var financialYear = await _financialYearRepository.GetFinancialYearByDateAsync(transactionDto.ParishId, transactionDto.TrDate);

            if (financialYear == null || financialYear.IsLocked)
            {
                _logger.LogError("Transactions for the financial year are locked. Cannot update transaction.");
                throw new InvalidOperationException("Transactions for this financial year are locked and cannot be modified.");
            }

            var transaction = _mapper.Map<Transaction>(transactionDto);
            var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
            return _mapper.Map<TransactionDto>(updatedTransaction);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting transaction with id: {id}", id);
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                _logger.LogError("Transaction with id {id} not found.", id);
                throw new InvalidOperationException("Transaction not found.");
            }

            var financialYear = await _financialYearRepository.GetFinancialYearByDateAsync(transaction.ParishId, transaction.TrDate);

            if (financialYear == null || financialYear.IsLocked)
            {
                _logger.LogError("Transactions for the financial year are locked. Cannot delete transaction.");
                throw new InvalidOperationException("Transactions for this financial year are locked and cannot be modified.");
            }

            await _transactionRepository.DeleteAsync(id);
            _logger.LogInformation("Transaction with id {id} deleted.", id);
        }
    }
}
