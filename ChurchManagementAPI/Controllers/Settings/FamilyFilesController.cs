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
    public class FamilyFilesController : ManagementAuthorizedTrialController
    {
        private readonly IFamilyFileService _familyFileService;
        private readonly ApplicationDbContext _context;

        public FamilyFilesController(
            IFamilyFileService familyFileService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _familyFileService = familyFileService ?? throw new ArgumentNullException(nameof(familyFileService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ?? Get all files for a family
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyFileDto>>> GetByFamily(
            [FromQuery] int familyId)
        {
            if (familyId <= 0)
            {
                return BadRequest(new { Error = "Invalid FamilyId", Message = "FamilyId must be a positive integer." });
            }

            var files = await _familyFileService.GetByFamilyAsync(familyId);
            return Ok(files);
        }

        // ?? Get all files for a member
        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<IEnumerable<FamilyFileDto>>> GetByMember(
            [FromQuery] int familyId,
            int memberId)
        {
            if (familyId <= 0)
            {
                return BadRequest(new { Error = "Invalid FamilyId", Message = "FamilyId must be a positive integer." });
            }

            if (memberId <= 0)
            {
                return BadRequest(new { Error = "Invalid MemberId", Message = "MemberId must be a positive integer." });
            }

            var files = await _familyFileService.GetByMemberAsync(familyId, memberId);
            return Ok(files);
        }

        // ?? Get files by type (ProfilePhoto, FamilyPhoto, etc.)
        [HttpGet("type/{fileType}")]
        public async Task<ActionResult<IEnumerable<FamilyFileDto>>> GetByType(
            [FromQuery] int familyId,
            [FromQuery] int? memberId,
            string fileType)
        {
            if (familyId <= 0)
            {
                return BadRequest(new { Error = "Invalid FamilyId", Message = "FamilyId must be a positive integer." });
            }

            if (memberId.HasValue && memberId.Value <= 0)
            {
                return BadRequest(new { Error = "Invalid MemberId", Message = "MemberId must be a positive integer." });
            }

            if (string.IsNullOrWhiteSpace(fileType))
            {
                return BadRequest(new { Error = "Invalid FileType", Message = "FileType is required." });
            }

            var files = await _familyFileService.GetByTypeAsync(familyId, memberId, fileType);
            return Ok(files);
        }

        // ?? Get single file metadata
        [HttpGet("{fileId}")]
        public async Task<ActionResult<FamilyFileDto>> GetById(Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid FileId", Message = "FileId cannot be empty." });
            }

            var file = await _familyFileService.GetByIdAsync(fileId);
            if (file == null)
            {
                return NotFound();
            }
            return Ok(file);
        }

        // ?? Save file metadata (after S3 upload)
        [HttpPost]
        public async Task<ActionResult<FamilyFileDto>> Create(
            [FromBody] FamilyFileCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationError = await ValidateFamilyExistsAsync(createDto.FamilyId);
            if (validationError != null)
            {
                return validationError;
            }

            if (createDto.MemberId.HasValue)
            {
                var memberValidation = await ValidateMemberExistsAsync(createDto.MemberId.Value);
                if (memberValidation != null)
                {
                    return memberValidation;
                }
            }

            var createdFile = await _familyFileService.AddAsync(createDto);
            return CreatedAtAction(
                nameof(GetById),
                new { fileId = createdFile.FileId },
                createdFile
            );
        }

        // ?? Approve file
        [HttpPut("{fileId}/approve")]
        public async Task<IActionResult> Approve(Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid FileId", Message = "FileId cannot be empty." });
            }

            try
            {
                await _familyFileService.ApproveAsync(fileId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // ?? Reject file
        [HttpPut("{fileId}/reject")]
        public async Task<IActionResult> Reject(Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid FileId", Message = "FileId cannot be empty." });
            }

            try
            {
                await _familyFileService.RejectAsync(fileId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // ?? Delete file
        [HttpDelete("{fileId}")]
        public async Task<IActionResult> Delete(Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid FileId", Message = "FileId cannot be empty." });
            }

            try
            {
                await _familyFileService.DeleteAsync(fileId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("presign-upload")]
        public async Task<ActionResult<PresignUploadResponseDto>> PresignUpload(
          [FromBody] PresignUploadRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var validationError = await ValidateFamilyExistsAsync(request.FamilyId);
            if (validationError != null)
            {
                return validationError;
            }

            if (request.MemberId.HasValue)
            {
                var memberValidation = await ValidateMemberExistsAsync(request.MemberId.Value);
                if (memberValidation != null)
                {
                    return memberValidation;
                }
            }

            var response = await _familyFileService.GenerateUploadUrlAsync(request);
            return Ok(response);
        }

        [HttpGet("{fileId}/signed-url")]
        public async Task<ActionResult<PresignDownloadResponseDto>> GetSignedUrl(Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid FileId", Message = "FileId cannot be empty." });
            }

            try
            {
                var response = await _familyFileService.GenerateDownloadUrlAsync(fileId);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        private async Task<BadRequestObjectResult?> ValidateFamilyExistsAsync(int familyId)
        {
            var familyExists = await _context.Families.AnyAsync(f => f.FamilyId == familyId);
            if (!familyExists)
            {
                return BadRequest(new { Error = "Invalid FamilyId", Message = $"Family with ID {familyId} does not exist." });
            }
            return null;
        }

        private async Task<BadRequestObjectResult?> ValidateMemberExistsAsync(int memberId)
        {
            var memberExists = await _context.FamilyMembers.AnyAsync(m => m.MemberId == memberId);
            if (!memberExists)
            {
                return BadRequest(new { Error = "Invalid MemberId", Message = $"FamilyMember with ID {memberId} does not exist." });
            }
            return null;
        }
    }
}
