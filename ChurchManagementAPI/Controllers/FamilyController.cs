using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FamilyController : ControllerBase
    {
        private readonly IFamilyService _familyService;

        public FamilyController(IFamilyService familyService)
        {
            _familyService = familyService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Family>>> GetFamilies([FromQuery] int? parishId, [FromQuery] int? unitId, [FromQuery] int? familyId)
        {
            var families = await _familyService.GetFamiliesAsync(parishId, unitId, familyId);
            return Ok(families);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Family>> GetById(int id)
        {
            var family = await _familyService.GetByIdAsync(id);
            if (family == null)
            {
                return NotFound();
            }
            return Ok(family);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<Family> requests)
        {
            try
            {
                var createdFamilies = await _familyService.AddOrUpdateAsync(requests);
                if (createdFamilies.Any())
                {
                    return CreatedAtAction(nameof(GetFamilies), createdFamilies);
                }
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Family family)
        {
            if (id != family.FamilyId)
            {
                return BadRequest();
            }

            await _familyService.UpdateAsync(family);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _familyService.DeleteAsync(id);
            return NoContent();
        }
    }

}
