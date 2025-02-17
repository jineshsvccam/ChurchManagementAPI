using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
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
        private readonly ILogger<DistrictController> _logger;

        public DistrictController(IDistrictService districtService, ILogger<DistrictController> logger)
        {
            _districtService = districtService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<DistrictDto>>> GetAll()
        {
            _logger.LogInformation("Fetching all districts.");
            var districts = await _districtService.GetAllAsync();
            _logger.LogInformation("Fetched {Count} districts.", districts?.Count());
            return Ok(districts);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DistrictDto>> GetById(int id)
        {
            var district = await _districtService.GetByIdAsync(id);
            if (district == null)
            {
                _logger.LogWarning("District with ID {Id} not found.", id);
                return NotFound();
            }
            return Ok(district);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(DistrictDto districtDto)
        {
            _logger.LogInformation("Creating new district: {@District}", districtDto);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid district model received.");
                return BadRequest(ModelState);
            }

            await _districtService.AddAsync(districtDto);
            _logger.LogInformation("District created with Name: {Name}", districtDto.DistrictName);
            return CreatedAtAction(nameof(GetById), new { id = districtDto.DistrictId }, districtDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DistrictDto>> Update(int id, DistrictDto districtDto)
        {
            _logger.LogInformation("Updating district with ID: {Id}", id);
            if (id != districtDto.DistrictId)
            {
                _logger.LogWarning("District ID mismatch. Expected {ExpectedId}, but received {ReceivedId}.", id, districtDto.DistrictId);
                return BadRequest();
            }

            await _districtService.UpdateAsync(districtDto);
            _logger.LogInformation("District with ID {Id} updated successfully.", id);

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
            _logger.LogInformation("Deleting district with ID: {Id}", id);
            await _districtService.DeleteAsync(id);
            _logger.LogInformation("District with ID {Id} deleted successfully.", id);
            return NoContent();
        }
    }
}
