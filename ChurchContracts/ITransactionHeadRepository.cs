using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface ITransactionHeadRepository
    {
        Task<IEnumerable<TransactionHead>> GetTransactionHeadsAsync(int? parishId, int? headId);
        Task<TransactionHead?> GetByIdAsync(int id);
        Task<TransactionHead> AddAsync(TransactionHead transactionHead, int userId); 
        Task<TransactionHead> UpdateAsync(TransactionHead transactionHead, int userId); 
        Task DeleteAsync(int id, int userId); 
    }
}
