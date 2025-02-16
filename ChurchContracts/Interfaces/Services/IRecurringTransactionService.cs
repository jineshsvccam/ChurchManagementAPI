using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData.DTOs;

namespace ChurchContracts
{
    public interface IRecurringTransactionService
    {
        Task<IEnumerable<RecurringTransactionDto>> GetAllAsync(int? parishId);
        Task<RecurringTransactionDto?> GetByIdAsync(int id);
        Task<RecurringTransactionDto> AddAsync(RecurringTransactionDto dto);
        Task<RecurringTransactionDto> UpdateAsync(int id, RecurringTransactionDto dto);
        Task DeleteAsync(int id);
    }
}
