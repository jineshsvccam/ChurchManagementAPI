using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{

    public class FamilyMemberController : FamilyMemberAuthorizedController
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

        // GET by id: returns complete family member JSON
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFamilyMemberById(int id)
        {
            var response = await _familyMemberService.GetFamilyMemberByIdAsync(id);
            if (!response.Success)
            {
                return NotFound(response.Message);
            }
            return Ok(response);
        }

        // POST filter: returns filtered list of family members with selected columns
        [HttpPost("filter")]
        public async Task<IActionResult> GetFamilyMembersFiltered([FromQuery] int parishId, [FromQuery] int? familyId, [FromBody] FamilyMemberFilterRequest filterRequest)
        {
            var response = await _familyMemberService.GetFamilyMembersFilteredAsync(parishId, familyId, filterRequest);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }
            return Ok(response);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllFamilyMembers([FromQuery] int? parishId, [FromQuery] int? familyId)
        {
            var response = await _familyMemberService.GetAllFamilyMembersAsync(parishId, familyId);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }
            return Ok(response);
        }

    }
}
