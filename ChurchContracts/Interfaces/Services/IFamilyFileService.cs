using System.Threading.Tasks;
using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IFamilyFileService
    {
        // 🔹 Retrieval
        Task<IEnumerable<FamilyFileDto>> GetByFamilyAsync(int familyId);
        Task<IEnumerable<FamilyFileDto>> GetByMemberAsync(int familyId, int memberId);
        Task<IEnumerable<FamilyFileDto>> GetByTypeAsync(
            int familyId,
            int? memberId,
            string fileType
        );

        Task<FamilyFileDto?> GetByIdAsync(Guid fileId);

        // 🔹 Creation
        Task<FamilyFileDto> AddAsync(FamilyFileCreateDto createDto);

        // 🔹 Status / Update
        Task ApproveAsync(Guid fileId);
        Task RejectAsync(Guid fileId);

        // 🔹 Deletion
        Task DeleteAsync(Guid fileId);

        // 🔹 Presigned Upload URL
        Task<PresignUploadResponseDto> GenerateUploadUrlAsync(PresignUploadRequestDto request);
        Task<PresignDownloadResponseDto> GenerateDownloadUrlAsync(Guid fileId);

    }
}
