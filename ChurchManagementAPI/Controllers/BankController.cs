using ChurchContracts;
using ChurchContracts.ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public async Task<ActionResult<IEnumerable<Bank>>> GetBanks([FromQuery] int? parishId, [FromQuery] int? bankId)
        {
            var banks = await _bankService.GetBanksAsync(parishId, bankId);
            return Ok(banks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Bank>> GetById(int id)
        {
            var bank = await _bankService.GetByIdAsync(id);
            if (bank == null)
            {
                return NotFound();
            }
            return Ok(bank);
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<Bank> requests)
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Bank bank)
        {
            if (id != bank.BankId)
            {
                return BadRequest();
            }

            await _bankService.UpdateAsync(bank);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _bankService.DeleteAsync(id);
            return NoContent();
        }
    }
}
