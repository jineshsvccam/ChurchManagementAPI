using ChurchContracts;
using ChurchData;
using ChurchServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DistrictController : ControllerBase
    {
        private readonly IDistrictService _districtService;

        public DistrictController(IDistrictService districtService)
        {
            _districtService = districtService;
        }

        [HttpGet]
        [Authorize(Roles= "Admin")]
        public async Task<ActionResult<IEnumerable<District>>> GetAll()
        {
            var districts = await _districtService.GetAllAsync();
            return Ok(districts);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(District district)
        {
            await _districtService.AddAsync(district);
            return CreatedAtAction(nameof(GetById), new { id = district.DistrictId }, district);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<District>> Update(int id, District district)
        {
            if (id != district.DistrictId)
            {
                return BadRequest();
            }

            await _districtService.UpdateAsync(district);

            var updatedDistrict = await _districtService.GetByIdAsync(id);
            if (updatedDistrict == null)
            {
                return NotFound();
            }

            return Ok(updatedDistrict);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            await _districtService.DeleteAsync(id);
            return NoContent();
        }
    }


}
