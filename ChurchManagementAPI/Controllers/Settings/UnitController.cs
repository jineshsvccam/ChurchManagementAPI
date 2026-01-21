using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Settings
{
    public class UnitController : ManagementAuthorizedTrialController
    {
        private readonly IUnitService _unitService;

        public UnitController(
            IUnitService unitService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnitDto>>> GetUnits([FromQuery] int? parishId)
        {
            var units = await _unitService.GetAllAsync(parishId);
            return Ok(units);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UnitDto>> GetById(int id)
        {
            var unit = await _unitService.GetByIdAsync(id);
            if (unit == null)
            {
                return NotFound();
            }
            return Ok(unit);
        }

        [HttpPost]
        public async Task<ActionResult<UnitDto>> Create([FromBody] UnitDto unitDto)
        {
            var createdUnit = await _unitService.AddAsync(unitDto);
            return CreatedAtAction(nameof(GetById), new { id = createdUnit.UnitId }, createdUnit);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UnitDto unitDto)
        {
            if (id != unitDto.UnitId)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                var updatedUnit = await _unitService.UpdateAsync(unitDto);
                return Ok(updatedUnit);
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
                await _unitService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<UnitDto> units)
        {
            if (units == null || !units.Any())
            {
                return BadRequest("Requests cannot be null or empty.");
            }

            try
            {
                var result = await _unitService.AddOrUpdateAsync(units);
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
