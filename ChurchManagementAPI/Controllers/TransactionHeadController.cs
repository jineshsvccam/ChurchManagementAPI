using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    public class TransactionHeadController : ManagementAuthorizedController<TransactionHeadController>
    {
        private readonly ITransactionHeadService _transactionHeadService;

        public TransactionHeadController(
            ITransactionHeadService transactionHeadService,          
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<TransactionHeadController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _transactionHeadService = transactionHeadService ?? throw new ArgumentNullException(nameof(transactionHeadService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionHeadDto>>> GetTransactionHeads([FromQuery] int? parishId, [FromQuery] int? headId)
        {
            var transactionHeads = await _transactionHeadService.GetTransactionHeadsAsync(parishId, headId);
            return Ok(transactionHeads);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionHeadDto>> GetById(int id)
        {
            var transactionHead = await _transactionHeadService.GetByIdAsync(id);
            if (transactionHead == null)
            {
                return NotFound();
            }
            return Ok(transactionHead);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionHeadDto>> Create([FromBody] TransactionHeadDto transactionHeadDto)
        {
            var createdHead = await _transactionHeadService.AddAsync(transactionHeadDto);
            return CreatedAtAction(nameof(GetById), new { id = createdHead.HeadId }, createdHead);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TransactionHeadDto transactionHeadDto)
        {
            if (id != transactionHeadDto.HeadId)
            {
                return BadRequest("ID mismatch");
            }

            var updatedHead = await _transactionHeadService.UpdateAsync(transactionHeadDto);
            if (updatedHead == null)
            {
                return NotFound();
            }

            return Ok(updatedHead);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _transactionHeadService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<TransactionHeadDto> transactionHeadDtos)
        {
            var createdTransactionHeads = await _transactionHeadService.AddOrUpdateAsync(transactionHeadDtos);
            if (createdTransactionHeads.Any())
            {
                return CreatedAtAction(nameof(GetTransactionHeads), createdTransactionHeads);
            }
            return Ok();
        }
    }
}
