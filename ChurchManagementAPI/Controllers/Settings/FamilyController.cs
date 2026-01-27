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
    public class FamilyController : ManagementAuthorizedTrialController
    {
        private readonly IFamilyService _familyService;
        private readonly ApplicationDbContext _context;

        public FamilyController(
            IFamilyService familyService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _familyService = familyService ?? throw new ArgumentNullException(nameof(familyService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyDto>>> GetFamilies([FromQuery] int? parishId, [FromQuery] int? unitId, [FromQuery] int? familyId)
        {
            if (parishId.HasValue && parishId.Value <= 0)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = "ParishId must be a positive integer." });
            }

            if (unitId.HasValue && unitId.Value <= 0)
            {
                return BadRequest(new { Error = "Invalid UnitId", Message = "UnitId must be a positive integer." });
            }

            var families = await _familyService.GetFamiliesAsync(parishId, unitId, familyId);
            return Ok(families);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FamilyDto>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Error = "Invalid Id", Message = "Id must be a positive integer." });
            }

            var family = await _familyService.GetByIdAsync(id);
            if (family == null)
            {
                return NotFound();
            }
            return Ok(family);
        }

        [HttpPost]
        public async Task<ActionResult<FamilyDto>> Create([FromBody] FamilyDto familyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationError = await ValidateForeignKeysAsync(familyDto.ParishId, familyDto.UnitId);
            if (validationError != null)
            {
                return validationError;
            }

            var createdFamily = await _familyService.AddAsync(familyDto);
            return CreatedAtAction(nameof(GetById), new { id = createdFamily.FamilyId }, createdFamily);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FamilyDto familyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != familyDto.FamilyId)
            {
                return BadRequest(new { Error = "ID mismatch", Message = "The ID in the URL does not match the ID in the request body." });
            }

            var validationError = await ValidateForeignKeysAsync(familyDto.ParishId, familyDto.UnitId);
            if (validationError != null)
            {
                return validationError;
            }

            try
            {
                var updatedFamily = await _familyService.UpdateAsync(familyDto);
                return Ok(updatedFamily);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Error = "Invalid Id", Message = "Id must be a positive integer." });
            }

            try
            {
                await _familyService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<FamilyDto> requests)
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest("Requests cannot be null or empty.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate all foreign keys
            var parishIds = requests.Select(r => r.ParishId).Distinct().ToList();
            var unitIds = requests.Select(r => r.UnitId).Distinct().ToList();

            var parishValidation = await ValidateParishIdsExistAsync(parishIds);
            if (parishValidation != null) return parishValidation;

            var unitValidation = await ValidateUnitIdsExistAsync(unitIds);
            if (unitValidation != null) return unitValidation;

            try
            {
                var result = await _familyService.AddOrUpdateAsync(requests);
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

        private async Task<BadRequestObjectResult?> ValidateForeignKeysAsync(int parishId, int unitId)
        {
            var parishExists = await _context.Parishes.AnyAsync(p => p.ParishId == parishId);
            if (!parishExists)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = $"Parish with ID {parishId} does not exist." });
            }

            var unitExists = await _context.Units.AnyAsync(u => u.UnitId == unitId);
            if (!unitExists)
            {
                return BadRequest(new { Error = "Invalid UnitId", Message = $"Unit with ID {unitId} does not exist." });
            }

            return null;
        }

        private async Task<BadRequestObjectResult?> ValidateParishIdsExistAsync(List<int> parishIds)
        {
            var existingIds = await _context.Parishes.Where(p => parishIds.Contains(p.ParishId)).Select(p => p.ParishId).ToListAsync();
            var invalidIds = parishIds.Except(existingIds).ToList();
            if (invalidIds.Any())
            {
                return BadRequest(new { Error = "Invalid ParishId(s)", Message = $"Parish(es) with ID(s) {string.Join(", ", invalidIds)} do not exist." });
            }
            return null;
        }

        private async Task<BadRequestObjectResult?> ValidateUnitIdsExistAsync(List<int> unitIds)
        {
            var existingIds = await _context.Units.Where(u => unitIds.Contains(u.UnitId)).Select(u => u.UnitId).ToListAsync();
            var invalidIds = unitIds.Except(existingIds).ToList();
            if (invalidIds.Any())
            {
                return BadRequest(new { Error = "Invalid UnitId(s)", Message = $"Unit(s) with ID(s) {string.Join(", ", invalidIds)} do not exist." });
            }
            return null;
        }
    }
}
