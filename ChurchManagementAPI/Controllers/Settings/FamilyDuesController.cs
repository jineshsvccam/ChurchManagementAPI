using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Settings
{
    public class FamilyDuesController : ManagementAuthorizedTrialController
    {
        private readonly IFamilyDueService _service;

        public FamilyDuesController(
            IFamilyDueService service,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyDueDto>>> GetAll([FromQuery] int? parishId)
        {
            var dues = await _service.GetAllAsync(parishId);
            return Ok(dues);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FamilyDueDto>> GetById(int id)
        {
            var due = await _service.GetByIdAsync(id);
            if (due == null)
            {
                return NotFound();
            }
            return Ok(due);
        }

        [HttpPost]
        public async Task<ActionResult<FamilyDueDto>> Create([FromBody] FamilyDueDto dto)
        {
            var createdDue = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdDue.DuesId }, createdDue);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FamilyDueDto dto)
        {
            if (id != dto.DuesId)
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
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<FamilyDueDto> requests)
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
