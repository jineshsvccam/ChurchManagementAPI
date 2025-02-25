using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{

    public class ContributionSettingsController : ManagementAuthorizedController
    {
        private readonly IContributionSettingsService _service;

        public ContributionSettingsController(IContributionSettingsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContributionSettingsDto>>> GetAll([FromQuery] int? parishId)
        {
            var result = await _service.GetAllAsync(parishId);
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

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
           await _service.DeleteAsync(id);
           
            return NoContent();
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<ContributionSettingsDto> requests)
        {
            var createdEntries = await _service.AddOrUpdateAsync(requests);
            if (createdEntries.Any())
            {
                return CreatedAtAction(nameof(GetAll), createdEntries);
            }
            return Ok(createdEntries);
        }
    }

}
