using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class TransactionHeadController : ControllerBase
    {
        private readonly ITransactionHeadService _transactionHeadService;

        public TransactionHeadController(ITransactionHeadService transactionHeadService)
        {
            _transactionHeadService = transactionHeadService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<ActionResult<IEnumerable<TransactionHead>>> GetTransactionHeads([FromQuery] int? parishId, [FromQuery] int? headId)
        {
            var transactionHeads = await _transactionHeadService.GetTransactionHeadsAsync(parishId, headId);
            return Ok(transactionHeads);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<ActionResult<TransactionHead>> GetById(int id)
        {
            var transactionHead = await _transactionHeadService.GetByIdAsync(id);
            if (transactionHead == null)
            {
                return NotFound();
            }
            return Ok(transactionHead);
        }
               
        [HttpPost]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<TransactionHead> requests)
        {
            try
            {             
                var createdTransactionHeads = await _transactionHeadService.AddOrUpdateAsync(requests);
                if (createdTransactionHeads.Any())
                {
                    return CreatedAtAction(nameof(GetTransactionHeads), createdTransactionHeads);
                }
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<IActionResult> Update(int id, [FromBody] TransactionHead transactionHead)
        {
            if (id != transactionHead.HeadId)
            {
                return BadRequest();
            }
            
            await _transactionHeadService.UpdateAsync(transactionHead);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Secretary,Trustee")]
        public async Task<IActionResult> Delete(int id)
        {           
            await _transactionHeadService.DeleteAsync(id);
            return NoContent();
        }
    }
}
