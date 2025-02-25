using ChurchContracts;
using ChurchDTOs.DTOs.Utils;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
   
    public class ReportsController : ManagementAuthorizedController
    {
        private readonly ILedgerService _ledgerService;
        private readonly IBankConsolidatedStatementService _bankService;

        public ReportsController(ILedgerService ledgerService, IBankConsolidatedStatementService bankService)
        {
            _ledgerService = ledgerService;
            _bankService = bankService;
        }

        [HttpGet("ledger")]
        public async Task<ActionResult<LedgerReportDTO>> GetLedger(
            [FromQuery] int parishId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool includeTransactions = false)
        {
            var ledger = await _ledgerService.GetLedgerAsync(parishId, startDate, endDate, includeTransactions);
            return Ok(ledger);
        }

        [HttpGet("bank-statement")]
        public async Task<ActionResult<BankStatementConsolidatedDTO>> GetBankStatement(
            [FromQuery] int parishId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] bool includeTransactions = false)
        {
            var bankStatement = await _bankService.GetBankStatementAsync(parishId, startDate, endDate, includeTransactions);
            return Ok(bankStatement);
        }

        // Add more report endpoints here...
    }
}
