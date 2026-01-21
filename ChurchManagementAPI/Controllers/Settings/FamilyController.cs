using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers.Settings
{
   // [ApiExplorerSettings(IgnoreApi = false)]
    public class FamilyController : ManagementAuthorizedTrialController
    {
        private readonly IFamilyService _familyService;

        public FamilyController(
            IFamilyService familyService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<FamilyController> logger)
           // : base(httpContextAccessor, context, logger)
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
                return NotFound($"Family with Id {id} not found.");
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
                return BadRequest("Family ID in the request body does not match the URL parameter.");
            }

            var updatedFamily = await _familyService.UpdateAsync(familyDto);
            if (updatedFamily == null)
            {
                return NotFound($"Family with Id {id} not found.");
            }

            return Ok(updatedFamily);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _familyService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<FamilyDto> requests)
        {
            var createdFamilies = await _familyService.AddOrUpdateAsync(requests);
            if (createdFamilies.Any())
            {
                return CreatedAtAction(nameof(GetFamilies), createdFamilies);
            }
            return Ok();
        }
    }
}
