using ChurchContracts;
using ChurchData.DTOs;
using System;
using System.Threading.Tasks;

namespace ChurchServices
{
    public class BankConsolidatedStatementService : IBankConsolidatedStatementService
    {
        private readonly IBankConsolidatedStatementRepository _bankRepository;

        public BankConsolidatedStatementService(IBankConsolidatedStatementRepository bankRepository)
        {
            _bankRepository = bankRepository;
        }

        public async Task<BankStatementConsolidatedDTO> GetBankStatementAsync(int parishId, DateTime? startDate, DateTime? endDate, bool includeTransactions = false)
        {
            return await _bankRepository.GetBankStatementAsync(parishId, startDate, endDate, includeTransactions);
        }
    }
}
