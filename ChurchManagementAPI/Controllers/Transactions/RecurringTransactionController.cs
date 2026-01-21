using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers.Transactions
{
   // [ApiExplorerSettings(IgnoreApi = false)]
    public class RecurringTransactionController : ManagementAuthorizedTrialController
    {
        private readonly IRecurringTransactionService _service;

        public RecurringTransactionController(IRecurringTransactionService service,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<RecurringTransactionController> logger)
           // : base(httpContextAccessor, context, logger)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecurringTransactionDto>>> GetAll([FromQuery] int? parishId)
        {
            return Ok(await _service.GetAllAsync(parishId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RecurringTransactionDto>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<RecurringTransactionDto>> Create(RecurringTransactionDto dto)
        {
            var created = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RecurringTransactionDto>> Update(int id, RecurringTransactionDto dto)
        {
            return Ok(await _service.UpdateAsync(id, dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<RecurringTransactionDto> requests)
        {
            var createdEntries = await _service.AddOrUpdateAsync(requests);
            if (createdEntries.Any())
            {
                return CreatedAtAction(nameof(GetAll), createdEntries);
            }
            return Ok();
        }

        [HttpDelete("by-parish-head")]
        public async Task<IActionResult> DeleteByParishAndHead([FromQuery] int parishId, [FromQuery] int headId)
        {
            try
            {
                int deletedCount = await _service.DeleteByParishAndHeadAsync(parishId, headId);
                return Ok(new { Message = $"{deletedCount} recurring transaction(s) deleted." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

    }
}
