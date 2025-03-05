using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
  //  [ApiExplorerSettings(IgnoreApi = false)]
    public class UnitController : ManagementAuthorizedTrialController
    {
        private readonly IUnitService _unitService;

        public UnitController(
             IUnitService unitService,
             IHttpContextAccessor httpContextAccessor,
             ApplicationDbContext context,
             ILogger<UnitController> logger)
            // : base(httpContextAccessor, context, logger)
        {
            _unitService = unitService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnitDto>>> GetUnits([FromQuery] int? parishId)
        {
            var unitsDto = await _unitService.GetAllAsync(parishId);
            return Ok(unitsDto);
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<UnitDto>> GetById(int id)
        {
            var unitDto = await _unitService.GetByIdAsync(id);
            if (unitDto == null)
            {
                return NotFound();
            }
            return Ok(unitDto);
        }

        
        [HttpPost]
        public async Task<ActionResult<UnitDto>> Create([FromBody] UnitDto unitDto)
        {
            var createdUnitDto = await _unitService.AddAsync(unitDto);
            return CreatedAtAction(nameof(GetById), new { id = createdUnitDto.UnitId }, createdUnitDto);
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UnitDto unitDto)
        {
            if (id != unitDto.UnitId)
            {
                return BadRequest();
            }

            var updatedUnitDto = await _unitService.UpdateAsync(unitDto);
            return Ok(updatedUnitDto);
        }

        // DELETE: api/unit/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _unitService.DeleteAsync(id);
            return NoContent();
        }

        // POST: api/unit/create-or-update
        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<UnitDto> units)
        {
            var processedUnitsDto = await _unitService.AddOrUpdateAsync(units);
            if (processedUnitsDto.Any())
            {
                return CreatedAtAction(nameof(GetUnits), processedUnitsDto);
            }
            return Ok();
        }
    }
}
