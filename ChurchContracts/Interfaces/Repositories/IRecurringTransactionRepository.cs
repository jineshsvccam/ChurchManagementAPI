using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface IRecurringTransactionRepository
    {
        Task<IEnumerable<RecurringTransaction>> GetAllAsync(int? parishId);
        Task<RecurringTransaction?> GetByIdAsync(int id);
        Task<RecurringTransaction> AddAsync(RecurringTransaction recurringTransaction);
        Task<RecurringTransaction> UpdateAsync(RecurringTransaction recurringTransaction);
        Task DeleteAsync(int id);
    }
}
