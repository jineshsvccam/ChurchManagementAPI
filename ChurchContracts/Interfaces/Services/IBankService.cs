using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    namespace ChurchContracts
    {
        public interface IBankService
        {
            Task<IEnumerable<Bank>> GetBanksAsync(int? parishId, int? bankId);
            Task<Bank?> GetByIdAsync(int id);
            Task<IEnumerable<Bank>> AddOrUpdateAsync(IEnumerable<Bank> requests);
            Task<Bank> UpdateAsync(Bank bank);
            Task DeleteAsync(int id);
            Task<Bank> AddAsync(Bank bank);
        }
    }

}
