using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Settings
{
    public class FamilyFilesController : ManagementAuthorizedTrialController
    {
        private readonly IFamilyFileService _familyFileService;

        public FamilyFilesController(
            IFamilyFileService familyFileService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _familyFileService = familyFileService ?? throw new ArgumentNullException(nameof(familyFileService));
        }

        // ?? Get all files for a family
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyFileDto>>> GetByFamily(
            [FromQuery] int familyId)
        {
            var files = await _familyFileService.GetByFamilyAsync(familyId);
            return Ok(files);
        }

        // ?? Get all files for a member
        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<IEnumerable<FamilyFileDto>>> GetByMember(
            [FromQuery] int familyId,
            int memberId)
        {
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
            var files = await _familyFileService.GetByTypeAsync(familyId, memberId, fileType);
            return Ok(files);
        }

        // ?? Get single file metadata
        [HttpGet("{fileId}")]
        public async Task<ActionResult<FamilyFileDto>> GetById(Guid fileId)
        {
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
            var response = await _familyFileService.GenerateUploadUrlAsync(request);
            return Ok(response);
        }

        [HttpGet("{fileId}/signed-url")]
        public async Task<ActionResult<PresignDownloadResponseDto>> GetSignedUrl(Guid fileId)
        {
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
    }
}
