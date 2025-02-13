using ChurchContracts;
using ChurchData;
using ChurchServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class FamilyController : ControllerBase
    {
        private readonly IFamilyService _familyService;
        private readonly ILogger<FamilyController> _logger;

        public FamilyController(IFamilyService familyService, ILogger<FamilyController> logger)
        {
            _familyService = familyService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<ActionResult<IEnumerable<Family>>> GetFamilies([FromQuery] int? parishId, [FromQuery] int? unitId, [FromQuery] int? familyId)
        {
            _logger.LogInformation("Fetching families with ParishId: {ParishId}, UnitId: {UnitId}, FamilyId: {FamilyId}", parishId, unitId, familyId);
            var families = await _familyService.GetFamiliesAsync(parishId, unitId, familyId);
            return Ok(families);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<ActionResult<Family>> GetById(int id)
        {
            _logger.LogInformation("Fetching family with Id: {FamilyId}", id);
            var family = await _familyService.GetByIdAsync(id);
            if (family == null)
            {
                _logger.LogWarning("Family with Id {FamilyId} not found.", id);
                return NotFound($"Family with Id {id} not found.");
            }
            return Ok(family);
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<ActionResult<Family>> Create(Family family)
        {
            var createdFamily = await _familyService.AddAsync(family);
            return CreatedAtAction(nameof(GetById), createdFamily);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<IActionResult> Update(int id, [FromBody] Family family)
        {
            _logger.LogInformation("Updating family with Id: {FamilyId}", id);

            if (id != family.FamilyId)
            {
                _logger.LogWarning("Family Id mismatch. Path Id: {PathId}, Body Id: {BodyId}", id, family.FamilyId);
                return BadRequest("Family ID in the request body does not match the URL parameter.");
            }

            var updatedFamily = await _familyService.UpdateAsync(family);
            if (updatedFamily == null)
            {
                _logger.LogWarning("Family with Id {FamilyId} not found for update.", id);
                return NotFound($"Family with Id {id} not found.");
            }

            _logger.LogInformation("Successfully updated family with Id {FamilyId}", id);
            return Ok(updatedFamily);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting family with Id: {FamilyId}", id);
            await _familyService.DeleteAsync(id);
            _logger.LogInformation("Successfully deleted family with Id {FamilyId}", id);
            return NoContent();
        }

        [HttpPost("create-or-update")]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<Family> requests)
        {
            _logger.LogInformation("Creating or updating {FamilyCount} families.", requests.Count());
            var createdFamilies = await _familyService.AddOrUpdateAsync(requests);
            if (createdFamilies.Any())
            {
                _logger.LogInformation("Successfully created/updated {FamilyCount} families.", createdFamilies.Count());
                return CreatedAtAction(nameof(GetFamilies), createdFamilies);
            }
            return Ok();
        }
    }
}
