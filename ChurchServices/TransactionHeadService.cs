using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;

namespace ChurchServices
{
    public class TransactionHeadService : ITransactionHeadService
    {
        private readonly ITransactionHeadRepository _transactionHeadRepository;

        public TransactionHeadService(ITransactionHeadRepository transactionHeadRepository)
        {
            _transactionHeadRepository = transactionHeadRepository;
        }

        public async Task<IEnumerable<TransactionHead>> GetTransactionHeadsAsync(int? parishId, int? headId)
        {
            return await _transactionHeadRepository.GetTransactionHeadsAsync(parishId, headId);
        }

        public async Task<TransactionHead?> GetByIdAsync(int id)
        {
            return await _transactionHeadRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<TransactionHead>> AddOrUpdateAsync(IEnumerable<TransactionHead> requests)
        {
            var createdTransactionHeads = new List<TransactionHead>();

            foreach (var request in requests)
            {
                if (request.Action == "INSERT")
                {
                    var createdTransactionHead = await AddAsync(request);
                    createdTransactionHeads.Add(createdTransactionHead);
                }
                else if (request.Action == "UPDATE")
                {
                    var createdTransactionHead = await UpdateAsync(request);
                    createdTransactionHeads.Add(createdTransactionHead);
                }
                else
                {
                    throw new ArgumentException("Invalid action specified");
                }
            }
            return createdTransactionHeads;
        }

        public async Task<TransactionHead> AddAsync(TransactionHead transactionHead)
        {
            var addedTransactionHead = await _transactionHeadRepository.AddAsync(transactionHead);
            return addedTransactionHead;
        }

        public async Task<TransactionHead> UpdateAsync(TransactionHead transactionHead)
        {
            var updatedTransactionHead = await _transactionHeadRepository.UpdateAsync(transactionHead);
            return updatedTransactionHead;
        }

        public async Task DeleteAsync(int id)
        {
            await _transactionHeadRepository.DeleteAsync(id);
        }
    }
}
