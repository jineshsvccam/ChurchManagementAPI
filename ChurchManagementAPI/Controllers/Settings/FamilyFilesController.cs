using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers.Settings
{
    public class FamilyFilesController : ManagementAuthorizedTrialController
    {
        private readonly IFamilyFileService _familyFileService;

        public FamilyFilesController(
            IFamilyFileService familyFileService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<FamilyFilesController> logger)
        // : base(httpContextAccessor, context, logger)
        {
            _familyFileService = familyFileService
                ?? throw new ArgumentNullException(nameof(familyFileService));
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
                return NotFound($"Family file with Id {fileId} not found.");
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
            await _familyFileService.ApproveAsync(fileId);
            return NoContent();
        }

        // ?? Reject file
        [HttpPut("{fileId}/reject")]
        public async Task<IActionResult> Reject(Guid fileId)
        {
            await _familyFileService.RejectAsync(fileId);
            return NoContent();
        }

        // ?? Delete file
        [HttpDelete("{fileId}")]
        public async Task<IActionResult> Delete(Guid fileId)
        {
            await _familyFileService.DeleteAsync(fileId);
            return NoContent();
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
            var response = await _familyFileService.GenerateDownloadUrlAsync(fileId);
            return Ok(response);
        }


    }
}
