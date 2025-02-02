using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ChurchContracts;
    using ChurchContracts.ChurchContracts;
    using ChurchData;

    namespace ChurchServices
    {
        public class BankService : IBankService
        {
            private readonly IBankRepository _bankRepository;

            public BankService(IBankRepository bankRepository)
            {
                _bankRepository = bankRepository;
            }

            public async Task<IEnumerable<Bank>> GetBanksAsync(int? parishId, int? bankId)
            {
                return await _bankRepository.GetBanksAsync(parishId, bankId);
            }

            public async Task<Bank?> GetByIdAsync(int id)
            {
                return await _bankRepository.GetByIdAsync(id);
            }

            public async Task<IEnumerable<Bank>> AddOrUpdateAsync(IEnumerable<Bank> requests)
            {
                var createdBanks = new List<Bank>();

                foreach (var request in requests)
                {
                    if (request.Action == "INSERT")
                    {
                        var createdBank = await AddAsync(request);
                        createdBanks.Add(createdBank);
                    }
                    else if (request.Action == "UPDATE")
                    {
                        var updatedBank = await UpdateAsync(request);
                        createdBanks.Add(updatedBank);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid action specified");
                    }
                }
                return createdBanks;
            }

            public async Task<Bank> AddAsync(Bank bank)
            {
                var addedBank = await _bankRepository.AddAsync(bank);
                return addedBank;
            }

            public async Task<Bank> UpdateAsync(Bank bank)
            {
                var updatedBank = await _bankRepository.UpdateAsync(bank);
                return updatedBank;
            }

            public async Task DeleteAsync(int id)
            {
                await _bankRepository.DeleteAsync(id);
            }
        }
    }

}
