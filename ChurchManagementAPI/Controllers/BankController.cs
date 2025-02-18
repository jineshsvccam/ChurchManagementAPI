using ChurchContracts.ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankController : ControllerBase
    {
        private readonly IBankService _bankService;

        public BankController(IBankService bankService)
        {
            _bankService = bankService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BankDto>>> GetBanks([FromQuery] int? parishId, [FromQuery] int? bankId)
        {
            var banks = await _bankService.GetBanksAsync(parishId, bankId);
            return Ok(banks);
        }

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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BankDto bank)
        {
            if (id != bank.BankId)
            {
                return BadRequest();
            }

            await _bankService.UpdateAsync(bank);
            return Ok(bank);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _bankService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<BankDto> requests)
        {
            try
            {
                var createdBanks = await _bankService.AddOrUpdateAsync(requests);
                if (createdBanks.Any())
                {
                    return CreatedAtAction(nameof(GetBanks), createdBanks);
                }
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
