using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{

    public class FamilyContributionController : ManagementAuthorizedController<FamilyContributionController>
    {
        private readonly IFamilyContributionService _service;

        public FamilyContributionController(IFamilyContributionService service,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<FamilyContributionController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyContributionDto>>> GetAll([FromQuery] int? parishId)
        {
            return Ok(await _service.GetAllAsync(parishId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FamilyContributionDto>> GetById(int id)
        {
            var contribution = await _service.GetByIdAsync(id);
            if (contribution == null) return NotFound();
            return Ok(contribution);
        }

        [HttpPost]
        public async Task<ActionResult<FamilyContributionDto>> Add(FamilyContributionDto dto)
        {
            var createdContribution = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdContribution.SettingId }, createdContribution);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FamilyContributionDto>> Update(int id, FamilyContributionDto dto)
        {
            if (id <= 0)
                return BadRequest("Invalid ID");

            dto.ContributionId = id; // Assign ID from the URL to the DTO

            var updatedContribution = await _service.UpdateAsync(dto);
            return updatedContribution != null ? Ok(updatedContribution) : NotFound("Family Contribution not found");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<FamilyContributionDto> requests)
        {
            var createdEntries = await _service.AddOrUpdateAsync(requests);
            if (createdEntries.Any())
            {
                return CreatedAtAction(nameof(GetAll), createdEntries);
            }
            return Ok();
        }
    }
}
