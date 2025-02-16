using ChurchData;
using ChurchData.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchContracts
{
    public interface ITransactionRepository
    {
        Task<PagedResult<Transaction>> GetTransactionsAsync(int? parishId, int? familyId, int? transactionId, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize);
        Task<Transaction?> GetByIdAsync(int id);
        Task<Transaction> AddAsync(Transaction transaction);
        Task<Transaction> UpdateAsync(Transaction transaction);
        Task DeleteAsync(int id);
    }
}
