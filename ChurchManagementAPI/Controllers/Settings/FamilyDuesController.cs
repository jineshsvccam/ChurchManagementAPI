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
    public class FamilyDuesController : ManagementAuthorizedTrialController
    {
        private readonly IFamilyDueService _service;
        private readonly ApplicationDbContext _context;

        public FamilyDuesController(
            IFamilyDueService service,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyDueDto>>> GetAll([FromQuery] int? parishId)
        {
            if (parishId.HasValue && parishId.Value <= 0)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = "ParishId must be a positive integer." });
            }

            var dues = await _service.GetAllAsync(parishId);
            return Ok(dues);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FamilyDueDto>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Error = "Invalid Id", Message = "Id must be a positive integer." });
            }

            var due = await _service.GetByIdAsync(id);
            if (due == null)
            {
                return NotFound();
            }
            return Ok(due);
        }

        [HttpPost]
        public async Task<ActionResult<FamilyDueDto>> Create([FromBody] FamilyDueDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationError = await ValidateForeignKeysAsync(dto.ParishId, dto.FamilyId, dto.HeadId);
            if (validationError != null)
            {
                return validationError;
            }

            var createdDue = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdDue.DuesId }, createdDue);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] FamilyDueDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != dto.DuesId)
            {
                return BadRequest(new { Error = "ID mismatch", Message = "The ID in the URL does not match the ID in the request body." });
            }

            var validationError = await ValidateForeignKeysAsync(dto.ParishId, dto.FamilyId, dto.HeadId);
            if (validationError != null)
            {
                return validationError;
            }

            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                return Ok(updated);
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
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<FamilyDueDto> requests)
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
            var familyIds = requests.Select(r => r.FamilyId).Distinct().ToList();
            var headIds = requests.Select(r => r.HeadId).Distinct().ToList();

            var parishValidation = await ValidateParishIdsExistAsync(parishIds);
            if (parishValidation != null) return parishValidation;

            var familyValidation = await ValidateFamilyIdsExistAsync(familyIds);
            if (familyValidation != null) return familyValidation;

            var headValidation = await ValidateHeadIdsExistAsync(headIds);
            if (headValidation != null) return headValidation;

            try
            {
                var result = await _service.AddOrUpdateAsync(requests);
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

        private async Task<BadRequestObjectResult?> ValidateForeignKeysAsync(int parishId, int familyId, int headId)
        {
            var parishExists = await _context.Parishes.AnyAsync(p => p.ParishId == parishId);
            if (!parishExists)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = $"Parish with ID {parishId} does not exist." });
            }

            var familyExists = await _context.Families.AnyAsync(f => f.FamilyId == familyId);
            if (!familyExists)
            {
                return BadRequest(new { Error = "Invalid FamilyId", Message = $"Family with ID {familyId} does not exist." });
            }

            var headExists = await _context.TransactionHeads.AnyAsync(h => h.HeadId == headId);
            if (!headExists)
            {
                return BadRequest(new { Error = "Invalid HeadId", Message = $"TransactionHead with ID {headId} does not exist." });
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

        private async Task<BadRequestObjectResult?> ValidateFamilyIdsExistAsync(List<int> familyIds)
        {
            var existingIds = await _context.Families.Where(f => familyIds.Contains(f.FamilyId)).Select(f => f.FamilyId).ToListAsync();
            var invalidIds = familyIds.Except(existingIds).ToList();
            if (invalidIds.Any())
            {
                return BadRequest(new { Error = "Invalid FamilyId(s)", Message = $"Family(ies) with ID(s) {string.Join(", ", invalidIds)} do not exist." });
            }
            return null;
        }

        private async Task<BadRequestObjectResult?> ValidateHeadIdsExistAsync(List<int> headIds)
        {
            var existingIds = await _context.TransactionHeads.Where(h => headIds.Contains(h.HeadId)).Select(h => h.HeadId).ToListAsync();
            var invalidIds = headIds.Except(existingIds).ToList();
            if (invalidIds.Any())
            {
                return BadRequest(new { Error = "Invalid HeadId(s)", Message = $"TransactionHead(s) with ID(s) {string.Join(", ", invalidIds)} do not exist." });
            }
            return null;
        }
    }
}
