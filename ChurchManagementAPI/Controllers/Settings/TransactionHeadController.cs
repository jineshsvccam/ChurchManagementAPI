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
    public class TransactionHeadController : ManagementAuthorizedTrialController
    {
        private readonly ITransactionHeadService _transactionHeadService;
        private readonly ApplicationDbContext _context;

        public TransactionHeadController(
            ITransactionHeadService transactionHeadService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _transactionHeadService = transactionHeadService ?? throw new ArgumentNullException(nameof(transactionHeadService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionHeadDto>>> GetTransactionHeads([FromQuery] int? parishId, [FromQuery] int? headId)
        {
            if (parishId.HasValue && parishId.Value <= 0)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = "ParishId must be a positive integer." });
            }

            var transactionHeads = await _transactionHeadService.GetTransactionHeadsAsync(parishId, headId);
            return Ok(transactionHeads);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionHeadDto>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Error = "Invalid Id", Message = "Id must be a positive integer." });
            }

            var transactionHead = await _transactionHeadService.GetByIdAsync(id);
            if (transactionHead == null)
            {
                return NotFound();
            }
            return Ok(transactionHead);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionHeadDto>> Create([FromBody] TransactionHeadDto transactionHeadDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationError = await ValidateParishExistsAsync(transactionHeadDto.ParishId);
            if (validationError != null)
            {
                return validationError;
            }

            var createdHead = await _transactionHeadService.AddAsync(transactionHeadDto);
            return CreatedAtAction(nameof(GetById), new { id = createdHead.HeadId }, createdHead);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TransactionHeadDto transactionHeadDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != transactionHeadDto.HeadId)
            {
                return BadRequest(new { Error = "ID mismatch", Message = "The ID in the URL does not match the ID in the request body." });
            }

            var validationError = await ValidateParishExistsAsync(transactionHeadDto.ParishId);
            if (validationError != null)
            {
                return validationError;
            }

            try
            {
                var updatedHead = await _transactionHeadService.UpdateAsync(transactionHeadDto);
                return Ok(updatedHead);
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
                await _transactionHeadService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<TransactionHeadDto> transactionHeadDtos)
        {
            if (transactionHeadDtos == null || !transactionHeadDtos.Any())
            {
                return BadRequest("Requests cannot be null or empty.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var parishIds = transactionHeadDtos.Select(t => t.ParishId).Distinct().ToList();
            var validationError = await ValidateParishIdsExistAsync(parishIds);
            if (validationError != null)
            {
                return validationError;
            }

            try
            {
                var result = await _transactionHeadService.AddOrUpdateAsync(transactionHeadDtos);
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

        private async Task<BadRequestObjectResult?> ValidateParishExistsAsync(int parishId)
        {
            var parishExists = await _context.Parishes.AnyAsync(p => p.ParishId == parishId);
            if (!parishExists)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = $"Parish with ID {parishId} does not exist." });
            }
            return null;
        }

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
