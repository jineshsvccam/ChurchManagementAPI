using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers.Admin
{
    public class DioceseController : AdminAuthorizedController
    {
        private readonly IDioceseService _dioceseService;
        private readonly ILogger<DioceseController> _logger;

        public DioceseController(IDioceseService dioceseService, ILogger<DioceseController> logger)
        {
            _dioceseService = dioceseService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DioceseDto>>> GetAll()
        {
            _logger.LogInformation("Fetching all dioceses.");
            var dioceses = await _dioceseService.GetAllAsync();
            _logger.LogInformation("Fetched {Count} dioceses.", dioceses.Count());
            return Ok(dioceses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DioceseDto>> GetById(int id)
        {
            var diocese = await _dioceseService.GetByIdAsync(id);

            if (diocese == null)
            {
                _logger.LogWarning("Diocese with ID {Id} not found.", id);
                return NotFound();
            }

            return Ok(diocese);
        }

        [HttpPost]
        public async Task<ActionResult> Create(DioceseDto dioceseDto)
        {
            _logger.LogInformation("Creating new diocese: {@DioceseDto}", dioceseDto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid diocese model received.");
                return BadRequest(ModelState);
            }

            var createdDiocese = await _dioceseService.AddAsync(dioceseDto);

            _logger.LogInformation("Diocese created with ID: {Id}", createdDiocese.DioceseId);

            return CreatedAtAction(nameof(GetById), new { id = createdDiocese.DioceseId }, createdDiocese);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DioceseDto>> Update(int id, DioceseDto dioceseDto)
        {
            _logger.LogInformation("Updating diocese with ID: {Id}", id);
            if (id != dioceseDto.DioceseId)
            {
                _logger.LogWarning("Diocese ID mismatch. Expected {ExpectedId}, but received {ReceivedId}.", id, dioceseDto.DioceseId);
                return BadRequest();
            }

            await _dioceseService.UpdateAsync(dioceseDto);
            _logger.LogInformation("Diocese with ID {Id} updated successfully.", id);
            return Ok(await _dioceseService.GetByIdAsync(id));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting diocese with ID: {Id}", id);
            await _dioceseService.DeleteAsync(id);
            _logger.LogInformation("Diocese with ID {Id} deleted successfully.", id);
            return NoContent();
        }
    }
}
