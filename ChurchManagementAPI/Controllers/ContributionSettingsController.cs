using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/contribution-settings")]
    public class ContributionSettingsController : ControllerBase
    {
        private readonly IContributionSettingsService _service;

        public ContributionSettingsController(IContributionSettingsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result is null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ContributionSettingsDto dto)
        {
            var newId = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = newId }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ContributionSettingsDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
           
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
           await _service.DeleteAsync(id);
           
            return NoContent();
        }
    }

}
