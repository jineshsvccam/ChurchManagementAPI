using ChurchContracts;
using ChurchContracts.Utils;
using ChurchData;

namespace ChurchServices
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IFinancialYearRepository _financialYearRepository; // Inject FinancialYear repository

        public TransactionService(ITransactionRepository transactionRepository, IFinancialYearRepository financialYearRepository)
        {
            _transactionRepository = transactionRepository;
            _financialYearRepository = financialYearRepository;
        }

        public async Task<PagedResult<Transaction>> GetTransactionsAsync(int? parishId, int? familyId, int? transactionId, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            return await _transactionRepository.GetTransactionsAsync(parishId, familyId, transactionId, startDate, endDate, pageNumber, pageSize);
        }

        public async Task<Transaction?> GetByIdAsync(int id)
        {
            return await _transactionRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Transaction>> AddOrUpdateAsync(IEnumerable<Transaction> requests)
        {
            var createdTransactions = new List<Transaction>();

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
                    throw new ArgumentException("Invalid action specified");
                }
            }
            return createdTransactions;
        }

        public async Task<Transaction> AddAsync(Transaction transaction)
        {
            var financialYear = await _financialYearRepository.GetFinancialYearByDateAsync(transaction.ParishId, transaction.TrDate);

            if (financialYear == null || financialYear.IsLocked)
            {
                throw new InvalidOperationException("Transactions for this financial year are locked and cannot be modified.");
            }

            var addedTransaction = await _transactionRepository.AddAsync(transaction);
            return addedTransaction;
        }

        public async Task<Transaction> UpdateAsync(Transaction transaction)
        {
            var financialYear = await _financialYearRepository.GetFinancialYearByDateAsync(transaction.ParishId, transaction.TrDate);

            if (financialYear == null || financialYear.IsLocked)
            {
                throw new InvalidOperationException("Transactions for this financial year are locked and cannot be modified.");
            }

            var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
            return updatedTransaction;
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
        }
    }
}
