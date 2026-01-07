using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChurchDTOs.DTOs.Entities
{
    public class FamilyFileCreateDto
    {
        public int FamilyId { get; set; }
        public int? MemberId { get; set; }

        public string FileCategory { get; set; } = null!;
        public string FileType { get; set; } = null!;

        // S3 object key (NOT URL)
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
        public int FamilyId { get; set; }
        public int? MemberId { get; set; }

        public string FileType { get; set; } = null!;
        public string FileName { get; set; } = null!;
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
}
