using ChurchContracts;
using ChurchContracts.Utils;
using ChurchData;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<Transaction>>> GetTransactions(
           [FromQuery] int? parishId,
           [FromQuery] int? familyId,
           [FromQuery] int? transactionId,
           [FromQuery] DateTime? startDate,
           [FromQuery] DateTime? endDate,
           [FromQuery] int pageNumber = 1,
           [FromQuery] int pageSize = 50)
        {
            var transactions = await _transactionService.GetTransactionsAsync(parishId, familyId, transactionId, startDate, endDate, pageNumber, pageSize);
            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetById(int id)
        {
            var transaction = await _transactionService.GetByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<Transaction> requests)
        {
            try
            {
                var createdTransactions = await _transactionService.AddOrUpdateAsync(requests);
                if (createdTransactions.Any())
                {
                    return CreatedAtAction(nameof(GetTransactions), createdTransactions);
                }
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Transaction transaction)
        {
            if (id != transaction.TransactionId)
            {
                return BadRequest();
            }

            await _transactionService.UpdateAsync(transaction);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _transactionService.DeleteAsync(id);
            return NoContent();
        }
    }
}
