using ChurchContracts;
using ChurchData.DTOs;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitFamilyMember([FromBody] PendingFamilyMemberRequestDto requestDto)
        {
            var response = await _familyMemberService.SubmitFamilyMemberAsync(requestDto);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }
            return Ok(response);
        }

        [HttpPost("approval")]
        public async Task<IActionResult> ApproveFamilyMember([FromBody] FamilyMemberApprovalDto approvalDto)
        {
            var response = await _familyMemberService.ApproveFamilyMemberAsync(approvalDto);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }
            return Ok(response);
        }
    }
}
