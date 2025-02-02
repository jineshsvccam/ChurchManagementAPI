using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FamilyMemberController : ControllerBase
    {
        private readonly IFamilyMemberService _familyMemberService;

        public FamilyMemberController(IFamilyMemberService familyMemberService)
        {
            _familyMemberService = familyMemberService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyMember>>> GetFamilyMembers([FromQuery] int? parishId, [FromQuery] int? familyId, [FromQuery] int? memberId)
        {
            var familyMembers = await _familyMemberService.GetFamilyMembersAsync(parishId, familyId, memberId);
            return Ok(familyMembers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FamilyMember>> GetById(int id)
        {
            var familyMember = await _familyMemberService.GetByIdAsync(id);
            if (familyMember == null)
            {
                return NotFound();
            }
            return Ok(familyMember);
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<FamilyMember> requests)
        {
            try
            {
                var createdFamilyMembers = await _familyMemberService.AddOrUpdateAsync(requests);
                if (createdFamilyMembers.Any())
                {
                    return CreatedAtAction(nameof(GetFamilyMembers), createdFamilyMembers);
                }
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, FamilyMember familyMember)
        {
            if (id != familyMember.MemberId)
            {
                return BadRequest();
            }

            await _familyMemberService.UpdateAsync(familyMember);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _familyMemberService.DeleteAsync(id);
            return NoContent();
        }
    }
}
