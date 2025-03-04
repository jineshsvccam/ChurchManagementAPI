using ChurchContracts;
using ChurchContracts.Utils;
using ChurchData;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{

    public class TransactionController : ManagementAuthorizedController<TransactionController>
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<TransactionController> logger)
            : base(httpContextAccessor, context, logger)
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
        [HttpPost]
        public async Task<IActionResult> Create(TransactionDto transactionDto)
        {
            var createdTransaction = await _transactionService.AddAsync(transactionDto);
            return CreatedAtAction(nameof(GetTransactions), createdTransaction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TransactionDto transactionDto)
        {
            if (id != transactionDto.TransactionId)
            {
                return BadRequest();
            }

            await _transactionService.UpdateAsync(transactionDto);
            return Ok(transactionDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _transactionService.DeleteAsync(id);
            return NoContent();
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
    }
}
