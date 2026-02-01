using ChurchContracts;
using ChurchData;
using ChurchCommon.Utils;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Settings
{
    [Authorize(Policy = "FamilyMemberPolicy")]
    public class FamilyFilesController : ManagementAuthorizedTrialController
    {
        private readonly IFamilyFileService _familyFileService;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ManagementAuthorizedTrialController> _logger;

        public FamilyFilesController(
            IFamilyFileService familyFileService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _familyFileService = familyFileService ?? throw new ArgumentNullException(nameof(familyFileService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region GET Endpoints

        /// <summary>
        /// Get all files for a family
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FamilyFileDto>>> GetByFamily([FromQuery] int familyId)
        {
            if (familyId <= 0)
            {
                return BadRequest(new { Error = "Invalid FamilyId", Message = "FamilyId must be a positive integer." });
            }

            var (canAccess, errorMessage) = await CanAccessFamilyFilesAsync(familyId);
            if (!canAccess)
            {
                return Forbid(errorMessage);
            }

            var files = await _familyFileService.GetByFamilyAsync(familyId);
            return Ok(files);
        }

        /// <summary>
        /// Get all files for a specific family member
        /// </summary>
        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<IEnumerable<FamilyFileDto>>> GetByMember([FromQuery] int familyId, int memberId)
        {
            if (familyId <= 0)
            {
                return BadRequest(new { Error = "Invalid FamilyId", Message = "FamilyId must be a positive integer." });
            }

            if (memberId <= 0)
            {
                return BadRequest(new { Error = "Invalid MemberId", Message = "MemberId must be a positive integer." });
            }

            var (canAccess, errorMessage) = await CanAccessFamilyFilesAsync(familyId, memberId);
            if (!canAccess)
            {
                return Forbid(errorMessage);
            }

            var files = await _familyFileService.GetByMemberAsync(familyId, memberId);
            return Ok(files);
        }

        /// <summary>
        /// Get files filtered by type (ProfilePhoto, FamilyPhoto, etc.)
        /// </summary>
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

            var (canAccess, errorMessage) = await CanAccessFamilyFilesAsync(familyId, memberId);
            if (!canAccess)
            {
                return Forbid(errorMessage);
            }

            var files = await _familyFileService.GetByTypeAsync(familyId, memberId, fileType);
            return Ok(files);
        }

        /// <summary>
        /// Get single file metadata by ID
        /// </summary>
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

            var (canAccess, errorMessage) = await CanAccessFamilyFilesAsync(file.FamilyId);
            if (!canAccess)
            {
                return Forbid(errorMessage);
            }

            return Ok(file);
        }

        /// <summary>
        /// Get presigned download URL for a single file
        /// </summary>
        [HttpGet("{fileId}/signed-url")]
        public async Task<ActionResult<PresignDownloadResponseDto>> GetSignedUrl(Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid FileId", Message = "FileId cannot be empty." });
            }

            try
            {
                var file = await _familyFileService.GetByIdAsync(fileId);
                if (file == null)
                {
                    return NotFound();
                }

                var (canAccess, errorMessage) = await CanAccessFamilyFilesAsync(file.FamilyId);
                if (!canAccess)
                {
                    return Forbid(errorMessage);
                }

                var response = await _familyFileService.GenerateDownloadUrlAsync(fileId);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Get bulk presigned download URLs for all family files at once (optimized performance)
        /// </summary>
        [HttpGet("bulk-signed-urls")]
        public async Task<ActionResult<BulkPresignDownloadResponseDto>> GetBulkSignedUrls(
            [FromQuery] int familyId,
            [FromQuery] int? memberId = null)
        {
            if (familyId <= 0)
            {
                return BadRequest(new { Error = "Invalid FamilyId", Message = "FamilyId must be a positive integer." });
            }

            if (memberId.HasValue && memberId.Value <= 0)
            {
                return BadRequest(new { Error = "Invalid MemberId", Message = "MemberId must be a positive integer." });
            }

            var (canAccess, errorMessage) = await CanAccessFamilyFilesAsync(familyId, memberId);
            if (!canAccess)
            {
                return Forbid(errorMessage);
            }

            try
            {
                var response = await _familyFileService.GenerateBulkDownloadUrlsAsync(familyId, memberId);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        #endregion

        #region POST Endpoints

        /// <summary>
        /// Save file metadata after S3 upload (max 12 files per family)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FamilyFileDto>> Create([FromBody] FamilyFileCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (canAccess, errorMessage) = await CanAccessFamilyFilesAsync(createDto.FamilyId, createDto.MemberId);
            if (!canAccess)
            {
                return Forbid(errorMessage);
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

            try
            {
                var createdFile = await _familyFileService.AddAsync(createDto);
                return CreatedAtAction(
                    nameof(GetById),
                    new { fileId = createdFile.FileId },
                    createdFile
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = "Validation Error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Get presigned S3 upload URL for file upload
        /// </summary>
        [HttpPost("presign-upload")]
        public async Task<ActionResult<PresignUploadResponseDto>> PresignUpload([FromBody] PresignUploadRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (canAccess, errorMessage) = await CanAccessFamilyFilesAsync(request.FamilyId, request.MemberId);
            if (!canAccess)
            {
                return Forbid(errorMessage);
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

        #endregion

        #region PUT Endpoints

        /// <summary>
        /// Update file metadata (FileType, FileCategory, IsPrimary)
        /// </summary>
        [HttpPut("{fileId}")]
        public async Task<ActionResult<FamilyFileDto>> Update(Guid fileId, [FromBody] FamilyFileUpdateDto updateDto)
        {
            if (fileId == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid FileId", Message = "FileId cannot be empty." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var file = await _familyFileService.GetByIdAsync(fileId);
                if (file == null)
                {
                    return NotFound();
                }

                // If update specifies MemberId, validate the member exists
                if (updateDto.MemberId.HasValue)
                {
                    var memberValidation = await ValidateMemberExistsAsync(updateDto.MemberId.Value);
                    if (memberValidation != null)
                    {
                        return memberValidation;
                    }
                }

                var (canAccess, errorMessage) = await CanAccessFamilyFilesAsync(file.FamilyId, updateDto.MemberId);
                if (!canAccess)
                {
                    return Forbid(errorMessage);
                }

                var updatedFile = await _familyFileService.UpdateAsync(fileId, updateDto);
                return Ok(updatedFile);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Approve file (changes status to Approved)
        /// </summary>
        [HttpPut("{fileId}/approve")]
        public async Task<IActionResult> Approve(Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid FileId", Message = "FileId cannot be empty." });
            }

            try
            {
                var file = await _familyFileService.GetByIdAsync(fileId);
                if (file == null)
                {
                    return NotFound();
                }

                var (canAccess, errorMessage) = await CanAccessFamilyFilesAsync(file.FamilyId);
                if (!canAccess)
                {
                    return Forbid(errorMessage);
                }

                await _familyFileService.ApproveAsync(fileId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Reject file (changes status to Rejected)
        /// </summary>
        [HttpPut("{fileId}/reject")]
        public async Task<IActionResult> Reject(Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid FileId", Message = "FileId cannot be empty." });
            }

            try
            {
                var file = await _familyFileService.GetByIdAsync(fileId);
                if (file == null)
                {
                    return NotFound();
                }

                var (canAccess, errorMessage) = await CanAccessFamilyFilesAsync(file.FamilyId);
                if (!canAccess)
                {
                    return Forbid(errorMessage);
                }

                await _familyFileService.RejectAsync(fileId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        #endregion

        #region DELETE Endpoints

        /// <summary>
        /// Delete file
        /// </summary>
        [HttpDelete("{fileId}")]
        public async Task<IActionResult> Delete(Guid fileId)
        {
            if (fileId == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid FileId", Message = "FileId cannot be empty." });
            }

            try
            {
                var file = await _familyFileService.GetByIdAsync(fileId);
                if (file == null)
                {
                    return NotFound();
                }

                var (canAccess, errorMessage) = await CanAccessFamilyFilesAsync(file.FamilyId);
                if (!canAccess)
                {
                    return Forbid(errorMessage);
                }

                await _familyFileService.DeleteAsync(fileId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Check if user can access family files based on role, parish, and family ownership
        /// Implements three-layer access control:
        /// 1. Parish ownership validation (non-admin users)
        /// 2. Role-based permissions
        /// 3. Family isolation for FamilyMembers
        /// </summary>
        private async Task<(bool canAccess, string? errorMessage)> CanAccessFamilyFilesAsync(int familyId, int? memberId = null)
        {
            var (roleName, userParishId, userFamilyId) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);

            // Fetch family to validate parish ownership
            var family = await _context.Families.FindAsync(familyId);
            if (family == null)
            {
                return (false, "Family not found.");
            }

            // Validate parish ownership for non-admin users
            if (!roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (userParishId != family.ParishId)
                {
                    return (false, "You are not authorized to access files from another parish.");
                }
            }

            // FamilyMember can only access their own family
            if (roleName.Equals("FamilyMember", StringComparison.OrdinalIgnoreCase))
            {
                if (userFamilyId != familyId)
                {
                    return (false, "You are not authorized to access files from other families.");
                }
            }

            return (true, null);
        }

        /// <summary>
        /// Validate that a family exists in the database
        /// </summary>
        private async Task<BadRequestObjectResult?> ValidateFamilyExistsAsync(int familyId)
        {
            var familyExists = await _context.Families.AnyAsync(f => f.FamilyId == familyId);
            if (!familyExists)
            {
                return BadRequest(new { Error = "Invalid FamilyId", Message = $"Family with ID {familyId} does not exist." });
            }
            return null;
        }

        /// <summary>
        /// Validate that a family member exists in the database
        /// </summary>
        private async Task<BadRequestObjectResult?> ValidateMemberExistsAsync(int memberId)
        {
            var memberExists = await _context.FamilyMembers.AnyAsync(m => m.MemberId == memberId);
            if (!memberExists)
            {
                return BadRequest(new { Error = "Invalid MemberId", Message = $"FamilyMember with ID {memberId} does not exist." });
            }
            return null;
        }

        #endregion
    }
}
