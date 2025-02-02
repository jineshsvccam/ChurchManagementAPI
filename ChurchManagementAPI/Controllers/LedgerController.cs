using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData.DTOs;
using ChurchServices;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LedgerController : ControllerBase
    {
        private readonly ILedgerService _ledgerService;

        public LedgerController(ILedgerService ledgerService)
        {
            _ledgerService = ledgerService;
        }

        [HttpGet("ledger")]
        public async Task<ActionResult<LedgerReportDTO>> GetLedger(
             [FromQuery] int parishId,
             [FromQuery] DateTime? startDate,
             [FromQuery] DateTime? endDate,
             [FromQuery] bool includeTransactions=false)
        {
            var ledger = await _ledgerService.GetLedgerAsync(parishId, startDate, endDate, includeTransactions);
            return Ok(ledger);
        }
    }
}
