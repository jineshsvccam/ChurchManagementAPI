using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData.DTOs;
using ChurchRepositories;

namespace ChurchServices
{
    public class LedgerService : ILedgerService
    {
        private readonly ILedgerRepository _ledgerRepository;

        public LedgerService(ILedgerRepository ledgerRepository)
        {
            _ledgerRepository = ledgerRepository;
        }

        public async Task<IEnumerable<LedgerReportDTO>> GetLedgerAsync(int parishId, DateTime? startDate, DateTime? endDate, bool includeTransactions = false)
        {
            return await _ledgerRepository.GetLedgerAsync(parishId, startDate, endDate, includeTransactions);
        }
    }
}
