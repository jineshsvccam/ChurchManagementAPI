using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParishController : ControllerBase
    {
        private readonly IParishService _parishService;

        public ParishController(IParishService parishService)
        {
            _parishService = parishService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Parish>>> GetAll()
        {
            var parishes = await _parishService.GetAllAsync();
            return Ok(parishes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Parish>> GetById(int id)
        {
            var parish = await _parishService.GetByIdAsync(id);
            if (parish == null)
            {
                return NotFound();
            }
            return Ok(parish);
        }

        [HttpPost]
        public async Task<ActionResult<Parish>> Create(Parish parish)
        {
            var createdParish = await _parishService.AddAsync(parish);
            return CreatedAtAction(nameof(GetById), new { id = createdParish.ParishId }, createdParish);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Parish parish)
        {
            if (id != parish.ParishId)
            {
                return BadRequest();
            }

            await _parishService.UpdateAsync(parish);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _parishService.DeleteAsync(id);
            return NoContent();
        }
        [HttpGet("{parishId}/details")]
        public async Task<ActionResult<ParishDetailsDto>> GetParishDetails(int parishId, [FromQuery] bool includeTransactions = false, [FromQuery] bool includeFamilyMembers = false)
        {
            var parishDetails = await _parishService.GetParishDetailsAsync(parishId, includeTransactions, includeFamilyMembers);

            if (parishDetails == null)
            {
                return NotFound();
            }

            return Ok(parishDetails);
        }
    }

}
