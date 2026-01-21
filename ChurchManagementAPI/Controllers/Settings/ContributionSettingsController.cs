using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Settings
{
    public class ContributionSettingsController : ManagementAuthorizedTrialController
    {
        private readonly IContributionSettingsService _service;

        public ContributionSettingsController(
            IContributionSettingsService service,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContributionSettingsDto>>> GetAll([FromQuery] int? parishId)
        {
            var result = await _service.GetAllAsync(parishId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContributionSettingsDto>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ContributionSettingsDto>> Create([FromBody] ContributionSettingsDto dto)
        {
            var created = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.SettingId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ContributionSettingsDto dto)
        {
            if (id != dto.SettingId)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return Ok(updated);
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
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<ContributionSettingsDto> requests)
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest("Requests cannot be null or empty.");
            }

            try
            {
                var result = await _service.AddOrUpdateAsync(requests);
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
