using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Settings
{
    public class TransactionHeadController : ManagementAuthorizedTrialController
    {
        private readonly ITransactionHeadService _transactionHeadService;

        public TransactionHeadController(
            ITransactionHeadService transactionHeadService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
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

            try
            {
                var updatedHead = await _transactionHeadService.UpdateAsync(transactionHeadDto);
                return Ok(updatedHead);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _transactionHeadService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<TransactionHeadDto> transactionHeadDtos)
        {
            if (transactionHeadDtos == null || !transactionHeadDtos.Any())
            {
                return BadRequest("Requests cannot be null or empty.");
            }

            try
            {
                var result = await _transactionHeadService.AddOrUpdateAsync(transactionHeadDtos);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
