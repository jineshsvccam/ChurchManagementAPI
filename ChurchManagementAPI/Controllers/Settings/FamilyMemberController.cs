using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Settings
{
    public class FamilyMemberController : FamilyMemberAuthorizedController
    {
        private readonly IFamilyMemberService _familyMemberService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FamilyMemberController> _logger;

        public FamilyMemberController(
            IFamilyMemberService familyMemberService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<FamilyMemberController> logger)
        {
            _familyMemberService = familyMemberService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _logger = logger;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitFamilyMember([FromBody] PendingFamilyMemberRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _familyMemberService.ApproveFamilyMemberAsync(approvalDto);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }
            return Ok(response);
        }

        [HttpGet("pending-approvals")]
        public async Task<IActionResult> GetPendingApprovalList([FromQuery] int parishId)
        {
            if (parishId <= 0)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = "ParishId must be a positive integer." });
            }

            var parishExists = await _context.Parishes.AnyAsync(p => p.ParishId == parishId);
            if (!parishExists)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = $"Parish with ID {parishId} does not exist." });
            }

            var response = await _familyMemberService.GetPendingApprovalListAsync(parishId);
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
            if (id <= 0)
            {
                return BadRequest(new { Error = "Invalid Id", Message = "Id must be a positive integer." });
            }

            var response = await _familyMemberService.GetFamilyMemberByIdAsync(id);
            if (!response.Success)
            {
                return NotFound(response.Message);
            }
            return Ok(response);
        }

        // POST filter: returns filtered list of family members with selected columns
        [HttpPost("filter")]
        public async Task<IActionResult> GetFamilyMembersFiltered(
            [FromQuery] int parishId,
            [FromQuery] int? familyId,
            [FromBody] FamilyMemberFilterRequest filterRequest)
        {
            if (parishId <= 0)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = "ParishId must be a positive integer." });
            }

            //if (familyId.HasValue && familyId.Value <= 0)
            //{
            //    return BadRequest(new { Error = "Invalid FamilyId", Message = "FamilyId must be a positive integer." });
            //}

            var parishExists = await _context.Parishes.AnyAsync(p => p.ParishId == parishId);
            if (!parishExists)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = $"Parish with ID {parishId} does not exist." });
            }

            if (familyId.HasValue && familyId>0)
            {
                var familyExists = await _context.Families.AnyAsync(f => f.FamilyId == familyId.Value);
                if (!familyExists)
                {
                    return BadRequest(new { Error = "Invalid FamilyId", Message = $"Family with ID {familyId.Value} does not exist." });
                }
            }

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
            if (parishId.HasValue && parishId.Value <= 0)
            {
                return BadRequest(new { Error = "Invalid ParishId", Message = "ParishId must be a positive integer." });
            }

            if (familyId.HasValue && familyId.Value <= 0)
            {
                return BadRequest(new { Error = "Invalid FamilyId", Message = "FamilyId must be a positive integer." });
            }

            if (parishId.HasValue)
            {
                var parishExists = await _context.Parishes.AnyAsync(p => p.ParishId == parishId.Value);
                if (!parishExists)
                {
                    return BadRequest(new { Error = "Invalid ParishId", Message = $"Parish with ID {parishId.Value} does not exist." });
                }
            }

            if (familyId.HasValue)
            {
                var familyExists = await _context.Families.AnyAsync(f => f.FamilyId == familyId.Value);
                if (!familyExists)
                {
                    return BadRequest(new { Error = "Invalid FamilyId", Message = $"Family with ID {familyId.Value} does not exist." });
                }
            }

            var response = await _familyMemberService.GetAllFamilyMembersAsync(parishId, familyId);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }
            return Ok(response);
        }
    }
}
