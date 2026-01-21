using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Settings
{
    public class FamilyController : ManagementAuthorizedTrialController
    {
        private readonly IFamilyService _familyService;

        public FamilyController(
            IFamilyService familyService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _familyService = familyService ?? throw new ArgumentNullException(nameof(familyService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyDto>>> GetFamilies([FromQuery] int? parishId, [FromQuery] int? unitId, [FromQuery] int? familyId)
        {
            var families = await _familyService.GetFamiliesAsync(parishId, unitId, familyId);
            return Ok(families);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FamilyDto>> GetById(int id)
        {
            var family = await _familyService.GetByIdAsync(id);
            if (family == null)
            {
                return NotFound();
            }
            return Ok(family);
        }

        [HttpPost]
        public async Task<ActionResult<FamilyDto>> Create([FromBody] FamilyDto familyDto)
        {
            var createdFamily = await _familyService.AddAsync(familyDto);
            return CreatedAtAction(nameof(GetById), new { id = createdFamily.FamilyId }, createdFamily);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FamilyDto familyDto)
        {
            if (id != familyDto.FamilyId)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                var updatedFamily = await _familyService.UpdateAsync(familyDto);
                return Ok(updatedFamily);
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
                await _familyService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<FamilyDto> requests)
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest("Requests cannot be null or empty.");
            }

            try
            {
                var result = await _familyService.AddOrUpdateAsync(requests);
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
