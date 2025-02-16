using ChurchContracts;
using ChurchDTOs.DTOs.Utils;

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
