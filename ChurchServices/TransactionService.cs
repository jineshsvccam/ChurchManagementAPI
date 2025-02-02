using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;

namespace ChurchServices
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(int? parishId, int? familyId, int? transactionId)
        {
            return await _transactionRepository.GetTransactionsAsync(parishId, familyId, transactionId);
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
            var addedTransaction = await _transactionRepository.AddAsync(transaction);
            return addedTransaction;
        }

        public async Task<Transaction> UpdateAsync(Transaction transaction)
        {
            var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
            return updatedTransaction;
        }

        public async Task DeleteAsync(int id)
        {
            await _transactionRepository.DeleteAsync(id);
        }
    }
}
