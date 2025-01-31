using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistrictController : ControllerBase
    {
        private readonly IDistrictService _districtService;

        public DistrictController(IDistrictService districtService)
        {
            _districtService = districtService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<District>>> GetAll()
        {
            var districts = await _districtService.GetAllAsync();
            return Ok(districts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<District>> GetById(int id)
        {
            var district = await _districtService.GetByIdAsync(id);
            if (district == null)
            {
                return NotFound();
            }
            return Ok(district);
        }

        [HttpPost]
        public async Task<ActionResult> Create(District district)
        {
            await _districtService.AddAsync(district);
            return CreatedAtAction(nameof(GetById), new { id = district.DistrictId }, district);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, District district)
        {
            if (id != district.DistrictId)
            {
                return BadRequest();
            }

            await _districtService.UpdateAsync(district);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _districtService.DeleteAsync(id);
            return NoContent();
        }
    }


}
