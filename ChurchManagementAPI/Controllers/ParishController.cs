using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ChurchData;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParishController : ControllerBase
    {
        private readonly IParishService _parishService;
        private readonly IMapper _mapper;
        private readonly ILogger<ParishController> _logger;

        public ParishController(IParishService parishService, IMapper mapper, ILogger<ParishController> logger)
        {
            _parishService = parishService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParishDto>>> GetAll()
        {
            _logger.LogInformation("Fetching all parishes.");
            var parishes = await _parishService.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<ParishDto>>(parishes));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ParishDto>> GetById(int id)
        {
            _logger.LogInformation("Fetching parish with ID {ParishId}.", id);
            var parish = await _parishService.GetByIdAsync(id);
            if (parish == null)
            {
                _logger.LogWarning("Parish with ID {ParishId} not found.", id);
                return NotFound();
            }
            return Ok(_mapper.Map<ParishDto>(parish));
        }

        [HttpPost]
        public async Task<ActionResult<ParishDto>> Create(ParishDto parishDto)
        {
            _logger.LogInformation("Creating a new parish: {ParishName}", parishDto.ParishName);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid Parish model received.");
                return BadRequest(ModelState);
            }
            var createdParish = await _parishService.AddAsync(parishDto);
            return CreatedAtAction(nameof(GetById), new { id = createdParish.ParishId }, _mapper.Map<ParishDto>(createdParish));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ParishDto parishDto)
        {
            if (id != parishDto.ParishId)
            {
                _logger.LogWarning("Parish ID mismatch: {ParishId}", id);
                return BadRequest();
            }

            
           var updatedParish= await _parishService.UpdateAsync(parishDto);
         
            if (updatedParish == null)
            {
                return NotFound();
            }
            _logger.LogInformation("Parish with ID {ParishId} updated successfully.", id);

            return Ok(updatedParish);
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting parish with ID {ParishId}.", id);
            await _parishService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("{parishId}/details")]
        public async Task<ActionResult<ParishDetailsDto>> GetParishDetails(int parishId, [FromQuery] bool includeFamilyMembers = false)
        {
            _logger.LogInformation("Fetching parish details for ID {ParishId}. IncludeFamilyMembers: {IncludeFamilyMembers}", parishId, includeFamilyMembers);
            var parishDetails = await _parishService.GetParishDetailsAsync(parishId, includeFamilyMembers);

            if (parishDetails == null)
            {
                _logger.LogWarning("Parish details not found for ID {ParishId}.", parishId);
                return NotFound();
            }

            return Ok(parishDetails);
        }
    }
}
