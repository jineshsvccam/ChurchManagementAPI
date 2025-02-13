using ChurchContracts;
using ChurchData;
using ChurchServices;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitController : ControllerBase
    {
        private readonly IUnitService _unitService;

        public UnitController(IUnitService unitService)
        {
            _unitService = unitService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Unit>>> GetUnits([FromQuery] int? parishId)
        {
            var units = await _unitService.GetAllAsync(parishId);
            return Ok(units);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Unit>> GetById(int id)
        {
            var unit = await _unitService.GetByIdAsync(id);
            if (unit == null)
            {
                return NotFound();
            }
            return Ok(unit);
        }

        [HttpPost]
        public async Task<ActionResult<Unit>> Create(Unit unit)
        {
            var createdUnit = await _unitService.AddAsync(unit);
            return CreatedAtAction(nameof(GetById), new { id = createdUnit.UnitId }, createdUnit);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Unit unit)
        {
            if (id != unit.UnitId)
            {
                return BadRequest();
            }

            await _unitService.UpdateAsync(unit);

            return Ok(unit);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _unitService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<Unit> units)
        {
            var createdUnits = await _unitService.AddOrUpdateAsync(units);
            if (units.Any())
            {
                return CreatedAtAction(nameof(GetUnits), createdUnits);
            }
            return Ok();
        }
    }

}
