using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DioceseController : ControllerBase
    {
        private readonly IDioceseService _dioceseService;

        public DioceseController(IDioceseService dioceseService)
        {
            _dioceseService = dioceseService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Diocese>>> GetAll()
        {
            var dioceses = await _dioceseService.GetAllAsync();
            return Ok(dioceses);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Diocese>> GetById(int id)
        {
            var diocese = await _dioceseService.GetByIdAsync(id);
            if (diocese == null)
            {
                return NotFound();
            }
            return Ok(diocese);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(Diocese diocese)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _dioceseService.AddAsync(diocese);
            return CreatedAtAction(nameof(GetById), new { id = diocese.DioceseId }, diocese);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Diocese>> Update(int id, Diocese diocese)
        {
            if (id != diocese.DioceseId)
            {
                return BadRequest();
            }

            await _dioceseService.UpdateAsync(diocese);

            // Get the updated diocese from the service
            var updatedDiocese = await _dioceseService.GetByIdAsync(id);

            if (updatedDiocese == null)
            {
                return NotFound();
            }

            return Ok(updatedDiocese);
        }



        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            await _dioceseService.DeleteAsync(id);
            return NoContent();
        }
    }


}
