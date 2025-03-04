using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    public class FamilyDuesController : ManagementAuthorizedController<FamilyDuesController>
    {
        private readonly IFamilyDueService _service;

        public FamilyDuesController(IFamilyDueService service,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<FamilyDuesController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyDueDto>>> GetAll([FromQuery] int? parishId)
        {
            return Ok(await _service.GetAllAsync(parishId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FamilyDueDto>> GetById(int id)
        {
            var due = await _service.GetByIdAsync(id);
            return due != null ? Ok(due) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<FamilyDueDto>> Create([FromBody] FamilyDueDto dto)
        {
            var createdDue = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdDue.FamilyId }, createdDue);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<FamilyDueDto>> Update(int id, [FromBody] FamilyDueDto dto)
        {
            return Ok(await _service.UpdateAsync(id, dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<FamilyDueDto> requests)
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
