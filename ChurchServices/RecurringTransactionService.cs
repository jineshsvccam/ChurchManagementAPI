using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;
using ChurchRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchServices
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly IRecurringTransactionRepository _repository;
        private readonly IMapper _mapper;

        public RecurringTransactionService(IRecurringTransactionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RecurringTransactionDto>> GetAllAsync(int? parishId)
        {
            var transactions = await _repository.GetAllAsync(parishId);
            return _mapper.Map<IEnumerable<RecurringTransactionDto>>(transactions);
        }

        public async Task<RecurringTransactionDto?> GetByIdAsync(int id)
        {
            var transaction = await _repository.GetByIdAsync(id);
            return transaction != null ? _mapper.Map<RecurringTransactionDto>(transaction) : null;
        }

        public async Task<RecurringTransactionDto> AddAsync(RecurringTransactionDto dto)
        {
            var transaction = _mapper.Map<RecurringTransaction>(dto);
            var addedTransaction = await _repository.AddAsync(transaction);
            return _mapper.Map<RecurringTransactionDto>(addedTransaction);
        }

        public async Task<RecurringTransactionDto> UpdateAsync(int id, RecurringTransactionDto dto)
        {
            var existingTransaction = await _repository.GetByIdAsync(id);
            if (existingTransaction == null)
                throw new KeyNotFoundException("Recurring Transaction not found");

            _mapper.Map(dto, existingTransaction);
            var updatedTransaction = await _repository.UpdateAsync(existingTransaction);
            return _mapper.Map<RecurringTransactionDto>(updatedTransaction);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
