using ChurchContracts.ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers.Settings
{
    [ApiExplorerSettings(IgnoreApi = false)]
    public class BankController : ManagementAuthorizedController<BankController>
    {
        private readonly IBankService _bankService;

        public BankController(
            IBankService bankService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<BankController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _bankService = bankService;
        }

        // This endpoint accepts a parishId in the query, which will be validated by the action filter.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BankDto>>> GetBanks([FromQuery] int parishId, [FromQuery] int? bankId)
        {
            var banks = await _bankService.GetBanksAsync(parishId, bankId);
            return Ok(banks);
        }

        // This endpoint does not accept a parishId parameter.
        // The filter will validate the returned BankDto (if it implements IParishEntity).
        [HttpGet("{id}")]
        public async Task<ActionResult<BankDto>> GetById(int id)
        {
            var bank = await _bankService.GetByIdAsync(id);
            if (bank == null)
            {
                return NotFound();
            }
            return Ok(bank);
        }

        [HttpPost]
        public async Task<ActionResult> Create(BankDto bank)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBank = await _bankService.AddAsync(bank);
            return CreatedAtAction(nameof(GetById), new { id = createdBank.BankId }, createdBank);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BankDto bank)
        {
            if (id != bank.BankId)
            {
                return BadRequest();
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
    }
}
