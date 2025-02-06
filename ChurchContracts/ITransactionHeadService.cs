using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface ITransactionHeadService
    {
        Task<IEnumerable<TransactionHead>> GetTransactionHeadsAsync(int? parishId, int? headId);
        Task<TransactionHead?> GetByIdAsync(int id);
        Task<IEnumerable<TransactionHead>> AddOrUpdateAsync(IEnumerable<TransactionHead> requests, int userId); // Updated method
        Task<TransactionHead> UpdateAsync(TransactionHead transactionHead, int userId); 
        Task DeleteAsync(int id, int userId); 
        Task<TransactionHead> AddAsync(TransactionHead transactionHead, int userId); 
    }
}
