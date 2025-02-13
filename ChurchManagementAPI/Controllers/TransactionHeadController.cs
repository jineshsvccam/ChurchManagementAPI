using ChurchContracts;
using ChurchData;
using ChurchServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionHeadController : ControllerBase
    {
        private readonly ITransactionHeadService _transactionHeadService;
        private readonly ILogger<TransactionHeadController> _logger;

        public TransactionHeadController(ITransactionHeadService transactionHeadService, ILogger<TransactionHeadController> logger)
        {
            _transactionHeadService = transactionHeadService ?? throw new ArgumentNullException(nameof(transactionHeadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<ActionResult<IEnumerable<TransactionHead>>> GetTransactionHeads([FromQuery] int? parishId, [FromQuery] int? headId)
        {
            _logger.LogInformation("Fetching transaction heads for ParishId: {ParishId}, HeadId: {HeadId}", parishId, headId);
            var transactionHeads = await _transactionHeadService.GetTransactionHeadsAsync(parishId, headId);
            return Ok(transactionHeads);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<ActionResult<TransactionHead>> GetById(int id)
        {
            _logger.LogInformation("Fetching transaction head by Id: {Id}", id);
            var transactionHead = await _transactionHeadService.GetByIdAsync(id);
            if (transactionHead == null)
            {
                _logger.LogWarning("Transaction head not found with Id: {Id}", id);
                return NotFound();
            }
            return Ok(transactionHead);
        }

        [HttpPost("create-or-update")]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<TransactionHead> requests)
        {
            _logger.LogInformation("Creating or updating transaction heads.");
            var createdTransactionHeads = await _transactionHeadService.AddOrUpdateAsync(requests);
            if (createdTransactionHeads.Any())
            {
                _logger.LogInformation("Successfully created or updated transaction heads.");
                return CreatedAtAction(nameof(GetTransactionHeads), createdTransactionHeads);
            }
            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<IActionResult> Update(int id, [FromBody] TransactionHead transactionHead)
        {
            if (id != transactionHead.HeadId)
            {
                _logger.LogWarning("Mismatch between URL Id: {Id} and body Id: {BodyId}", id, transactionHead.HeadId);
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating transaction head Id: {Id}", id);
            var updatedHead = await _transactionHeadService.UpdateAsync(transactionHead);
            if (updatedHead == null)
            {
                _logger.LogWarning("Transaction head not found for update with Id: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("Successfully updated transaction head Id: {Id}", id);
            return Ok(updatedHead);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting transaction head Id: {Id}", id);
            await _transactionHeadService.DeleteAsync(id);
            _logger.LogInformation("Successfully deleted transaction head Id: {Id}", id);
            return NoContent();
        }
    }
}
