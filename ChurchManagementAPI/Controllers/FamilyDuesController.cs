using ChurchContracts;
using ChurchData.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FamilyDuesController : ControllerBase
    {
        private readonly IFamilyDueService _service;

        public FamilyDuesController(IFamilyDueService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyDueDto>>> GetAll([FromQuery] int? parishId)
        {
            return Ok(await _service.GetAllAsync(parishId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FamilyDueDto>> GetById(int id)
        {
            var due = await _service.GetByIdAsync(id);
            return due != null ? Ok(due) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<FamilyDueDto>> Create([FromBody] FamilyDueDto dto)
        {
            var createdDue = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdDue.FamilyId }, createdDue);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FamilyDueDto>> Update(int id, [FromBody] FamilyDueDto dto)
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
