using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Settings
{
    public class UnitController : ManagementAuthorizedTrialController
    {
        private readonly IUnitService _unitService;
        private readonly ApplicationDbContext _context;

        public UnitController(
            IUnitService unitService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _unitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate that ParishId exists in the database
            var validationError = await ValidateParishExistsAsync(unitDto.ParishId);
            if (validationError != null)
            {
                return validationError;
            }

            var createdUnit = await _unitService.AddAsync(unitDto);
            return CreatedAtAction(nameof(GetById), new { id = createdUnit.UnitId }, createdUnit);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UnitDto unitDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != unitDto.UnitId)
            {
                return BadRequest("ID mismatch");
            }

            // Validate that ParishId exists in the database
            var validationError = await ValidateParishExistsAsync(unitDto.ParishId);
            if (validationError != null)
            {
                return validationError;
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

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate all ParishIds exist
            var parishIds = units.Select(u => u.ParishId).Distinct().ToList();
            var validationError = await ValidateParishIdsExistAsync(parishIds);
            if (validationError != null)
            {
                return validationError;
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

        /// <summary>
        /// Validates that a single ParishId exists in the database.
        /// </summary>
        private async Task<BadRequestObjectResult?> ValidateParishExistsAsync(int parishId)
        {
            var parishExists = await _context.Parishes.AnyAsync(p => p.ParishId == parishId);
            if (!parishExists)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = $"Parish with ID {parishId} does not exist." });
            }
            return null;
        }

        /// <summary>
        /// Validates that multiple ParishIds exist in the database.
        /// </summary>
        private async Task<BadRequestObjectResult?> ValidateParishIdsExistAsync(List<int> parishIds)
        {
            var existingParishIds = await _context.Parishes
                .Where(p => parishIds.Contains(p.ParishId))
                .Select(p => p.ParishId)
                .ToListAsync();

            var invalidParishIds = parishIds.Except(existingParishIds).ToList();
            if (invalidParishIds.Any())
            {
                return BadRequest(new { Error = "Invalid ParishId(s)", Message = $"Parish(es) with ID(s) {string.Join(", ", invalidParishIds)} do not exist." });
            }
            return null;
        }
    }
}
