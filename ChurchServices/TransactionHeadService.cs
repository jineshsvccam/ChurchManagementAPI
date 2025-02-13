using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using Microsoft.Extensions.Logging;

namespace ChurchServices
{
    public class TransactionHeadService : ITransactionHeadService
    {
        private readonly ITransactionHeadRepository _transactionHeadRepository;
        private readonly ILogger<TransactionHeadService> _logger;

        public TransactionHeadService(ITransactionHeadRepository transactionHeadRepository, ILogger<TransactionHeadService> logger)
        {
            _transactionHeadRepository = transactionHeadRepository ?? throw new ArgumentNullException(nameof(transactionHeadRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<TransactionHead>> GetTransactionHeadsAsync(int? parishId, int? headId)
        {
            _logger.LogInformation("Fetching transaction heads for ParishId: {ParishId}, HeadId: {HeadId}", parishId, headId);
            var result = await _transactionHeadRepository.GetTransactionHeadsAsync(parishId, headId);
            _logger.LogInformation("Fetched {Count} transaction heads successfully.", result?.ToString() ?? "0");
            return result;
        }

        public async Task<TransactionHead?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching transaction head by Id: {Id}", id);
            var result = await _transactionHeadRepository.GetByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Transaction head not found with Id: {Id}", id);
            }
            return result;
        }

        public async Task<IEnumerable<TransactionHead>> AddOrUpdateAsync(IEnumerable<TransactionHead> requests)
        {
            var createdTransactionHeads = new List<TransactionHead>();
            _logger.LogInformation("Processing {Count} transaction head(s) for AddOrUpdate.", requests?.ToString() ?? "0");

            foreach (var request in requests)
            {
                if (request.Action == "INSERT")
                {
                    _logger.LogInformation("Adding new transaction head: {HeadName}", request.HeadName);
                    var createdTransactionHead = await AddAsync(request);
                    createdTransactionHeads.Add(createdTransactionHead);
                }
                else if (request.Action == "UPDATE")
                {
                    _logger.LogInformation("Updating transaction head Id: {Id}", request.HeadId);
                    var createdTransactionHead = await UpdateAsync(request);
                    createdTransactionHeads.Add(createdTransactionHead);
                }
                else
                {
                    _logger.LogWarning("Invalid action '{Action}' specified for transaction head Id: {Id}", request.Action, request.HeadId);
                    throw new ArgumentException("Invalid action specified");
                }
            }

            _logger.LogInformation("Successfully processed {Count} transaction heads for AddOrUpdate.", createdTransactionHeads.Count);
            return createdTransactionHeads;
        }

        public async Task<TransactionHead> AddAsync(TransactionHead transactionHead)
        {
            _logger.LogInformation("Adding transaction head: {HeadName}", transactionHead.HeadName);
            var addedTransactionHead = await _transactionHeadRepository.AddAsync(transactionHead);
            _logger.LogInformation("Successfully added transaction head Id: {Id}", addedTransactionHead.HeadId);
            return addedTransactionHead;
        }

        public async Task<TransactionHead> UpdateAsync(TransactionHead transactionHead)
        {
            _logger.LogInformation("Updating transaction head Id: {Id}", transactionHead.HeadId);
            var updatedTransactionHead = await _transactionHeadRepository.UpdateAsync(transactionHead);
            _logger.LogInformation("Successfully updated transaction head Id: {Id}", updatedTransactionHead.HeadId);
            return updatedTransactionHead;
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting transaction head Id: {Id}", id);
            await _transactionHeadRepository.DeleteAsync(id);
            _logger.LogInformation("Successfully deleted transaction head Id: {Id}", id);
        }
    }
}
