using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchDTOs.DTOs.Entities
{
    public class FamilyFileCreateDto
    {
        [Required(ErrorMessage = "FamilyId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "FamilyId must be a positive integer.")]
        public int FamilyId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MemberId must be a positive integer when provided.")]
        public int? MemberId { get; set; }

        [Required(ErrorMessage = "FileCategory is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "FileCategory must be between 1 and 50 characters.")]
        public string FileCategory { get; set; } = null!;

        [Required(ErrorMessage = "FileType is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "FileType must be between 1 and 50 characters.")]
        public string FileType { get; set; } = null!;

        [Required(ErrorMessage = "FileKey is required.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "FileKey must be between 1 and 500 characters.")]
        public string FileKey { get; set; } = null!;

        public bool IsPrimary { get; set; }
    }
    public class FamilyFileDto
    {
        public Guid FileId { get; set; }

        public int FamilyId { get; set; }
        public int? MemberId { get; set; }

        public string FileCategory { get; set; } = null!;
        public string FileType { get; set; } = null!;

        public bool IsPrimary { get; set; }
        public string Status { get; set; } = null!;

        public DateTime UploadedAt { get; set; }
    }
    public class FamilyFileQueryDto
    {
        public int? FamilyId { get; set; }
        public int? MemberId { get; set; }
        public string? FileType { get; set; }
    }
    public class PresignUploadRequestDto
    {
        [Required(ErrorMessage = "FamilyId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "FamilyId must be a positive integer.")]
        public int FamilyId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MemberId must be a positive integer when provided.")]
        public int? MemberId { get; set; }

        [Required(ErrorMessage = "FileType is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "FileType must be between 1 and 50 characters.")]
        public string FileType { get; set; } = null!;

        [Required(ErrorMessage = "FileName is required.")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "FileName must be between 1 and 255 characters.")]
        public string FileName { get; set; } = null!;

        [Required(ErrorMessage = "ContentType is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "ContentType must be between 1 and 100 characters.")]
        public string ContentType { get; set; } = null!;
    }
    public class PresignUploadResponseDto
    {
        public string UploadUrl { get; set; } = null!;
        public string FileKey { get; set; } = null!;
    }
    public class PresignDownloadResponseDto
    {
        public string SignedUrl { get; set; } = null!;
        public int ExpiryMinutes { get; set; }
    }
    public class BulkPresignDownloadResponseDto
    {
        public IEnumerable<FileSignedUrlDto> Files { get; set; } = new List<FileSignedUrlDto>();
        public int ExpiryMinutes { get; set; }
    }
    public class FileSignedUrlDto
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public string SignedUrl { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
    }
    public class FamilyFileUpdateDto
    {
        [Required(ErrorMessage = "FileType is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "FileType must be between 1 and 50 characters.")]
        public string FileType { get; set; } = null!;

        [StringLength(50, MinimumLength = 1, ErrorMessage = "FileCategory must be between 1 and 50 characters.")]
        public string? FileCategory { get; set; }

        public bool? IsPrimary { get; set; }
    }
}
