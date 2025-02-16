using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecurringTransactionController : ControllerBase
    {
        private readonly IRecurringTransactionService _service;

        public RecurringTransactionController(IRecurringTransactionService service)
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
    }
}
