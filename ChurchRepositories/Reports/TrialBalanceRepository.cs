using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ChurchContracts;
using ChurchContracts.ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Utils;

namespace ChurchRepositories.Reports
{
    public class TrialBalanceRepository : ITrialBalancetRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILedgerService _ledgerService;
        private readonly IBankConsolidatedStatementService _bankService;

        public TrialBalanceRepository(ApplicationDbContext context, IMapper mapper, ILedgerService ledgerService,
            IBankConsolidatedStatementService bankService)
        {
            _context = context;
            _mapper = mapper;
            _ledgerService = ledgerService;
            _bankService = bankService;
        }
        public async Task<TrialBalanceDTO> GetTrialBalanceAsync(
             int parishId,
             DateTime? startDate,
             DateTime? endDate,
             bool includeTransactions = false,
             FinancialReportCustomizationOption customizationOption = FinancialReportCustomizationOption.Both)
        {
            // Get bank details as of the opening date:
            // Call the bank statement function with both start and end date as startDate
            var openingBankStatement = await _bankService.GetBankStatementAsync(
                parishId,
                startDate,
                startDate,
                false,
                customizationOption);

            // Get bank details as of the closing date:
            var closingBankStatement = await _bankService.GetBankStatementAsync(
                parishId,
                startDate,
                endDate,
                false,
                customizationOption);

            // Compute overall opening and closing balances by summing the closing balances of each call.
            // (When called with startDate as both parameters, the "closing_balance" reflects the balance as of startDate.)
            decimal openingBalance = openingBankStatement.Banks.Sum(b => b.ClosingBalance);
            decimal closingBalance = closingBankStatement.Banks.Sum(b => b.ClosingBalance);

            // Reuse the ledger service to get ledger details (heads and transactions)
            var ledgerReports = await _ledgerService.GetLedgerAsync(
                parishId,
                startDate,
                endDate,
                includeTransactions,
                customizationOption);
            var ledgerReport = ledgerReports;

            var trialBalance = new TrialBalanceDTO
            {
                ParishId = parishId,
                StartDate = startDate,
                EndDate = endDate,
                ReportName = "Trial Balance",

                OpeningBalance = openingBalance,
                ClosingBalance = closingBalance,
                OpeningDetails = openingBankStatement.Banks,
                ClosingDetails = closingBankStatement.Banks,
                Heads = ledgerReport?.Heads,
                Transactions =  new List<FinancialReportCustomDTO>()
            };
            return trialBalance;
        }
    }
}
