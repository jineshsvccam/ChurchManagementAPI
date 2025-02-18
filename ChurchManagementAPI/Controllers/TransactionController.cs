using ChurchContracts;
using ChurchContracts.Utils;
using ChurchDTOs.DTOs.Entities;
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
        public async Task<ActionResult<PagedResult<TransactionDto>>> GetTransactions(
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
        public async Task<ActionResult<TransactionDto>> GetById(int id)
        {
            var transaction = await _transactionService.GetByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<TransactionDto> requests)
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
        public async Task<IActionResult> Update(int id, TransactionDto transactionDto)
        {
            if (id != transactionDto.TransactionId)
            {
                return BadRequest();
            }

            await _transactionService.UpdateAsync(transactionDto);
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
