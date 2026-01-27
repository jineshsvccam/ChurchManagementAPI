using ChurchContracts.ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchManagementAPI.Controllers.Settings
{
    [ApiExplorerSettings(IgnoreApi = false)]
    public class BankController : ManagementAuthorizedController<BankController>
    {
        private readonly IBankService _bankService;
        private readonly ApplicationDbContext _context;

        public BankController(
            IBankService bankService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<BankController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _bankService = bankService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BankDto>>> GetBanks([FromQuery] int parishId, [FromQuery] int? bankId)
        {
            if (parishId <= 0)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = "ParishId must be a positive integer." });
            }

            var banks = await _bankService.GetBanksAsync(parishId, bankId);
            return Ok(banks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BankDto>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Error = "Invalid Id", Message = "Id must be a positive integer." });
            }

            var bank = await _bankService.GetByIdAsync(id);
            if (bank == null)
            {
                return NotFound();
            }
            return Ok(bank);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] BankDto bank)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationError = await ValidateParishExistsAsync(bank.ParishId);
            if (validationError != null)
            {
                return validationError;
            }

            var createdBank = await _bankService.AddAsync(bank);
            return CreatedAtAction(nameof(GetById), new { id = createdBank.BankId }, createdBank);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BankDto bank)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != bank.BankId)
            {
                return BadRequest(new { Error = "ID mismatch", Message = "The ID in the URL does not match the ID in the request body." });
            }

            var validationError = await ValidateParishExistsAsync(bank.ParishId);
            if (validationError != null)
            {
                return validationError;
            }

            try
            {
                await _bankService.UpdateAsync(bank);
                return Ok(bank);
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
                await _bankService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<BankDto> requests)
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest("Requests cannot be null or empty.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var parishIds = requests.Select(r => r.ParishId).Distinct().ToList();
            var validationError = await ValidateParishIdsExistAsync(parishIds);
            if (validationError != null)
            {
                return validationError;
            }

            try
            {
                var createdBanks = await _bankService.AddOrUpdateAsync(requests);
                return Ok(createdBanks);
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
