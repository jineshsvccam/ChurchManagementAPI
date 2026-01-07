using AutoMapper;
using ChurchContracts;
using ChurchData.Entities;
using ChurchDTOs.DTOs.Entities;
using ChurchRepositories;
using ChurchServices.Storage;
using Microsoft.Extensions.Logging;

namespace ChurchServices
{
    public class FamilyFileService : IFamilyFileService
    {
        private readonly IFamilyFileRepository _repository;
        private readonly ILogger<FamilyFileService> _logger;
        private readonly IMapper _mapper;
        private readonly IFamilyRepository _familyRepository;
        private readonly IFileStorageService _storageService;

        public FamilyFileService(
            IFamilyFileRepository repository,
            ILogger<FamilyFileService> logger,
            IMapper mapper,
            IFamilyRepository familyRepository,
            IFileStorageService storageService)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _familyRepository = familyRepository;
            _storageService = storageService;
        }

        public async Task<IEnumerable<FamilyFileDto>> GetByFamilyAsync(int familyId)
        {
            _logger.LogInformation("Fetching files for FamilyId: {FamilyId}", familyId);
            var files = await _repository.GetByFamilyAsync(familyId);
            return _mapper.Map<IEnumerable<FamilyFileDto>>(files);
        }

        public async Task<IEnumerable<FamilyFileDto>> GetByMemberAsync(int familyId, int memberId)
        {
            _logger.LogInformation(
                "Fetching files for FamilyId: {FamilyId}, MemberId: {MemberId}",
                familyId, memberId);

            var files = await _repository.GetByMemberAsync(familyId, memberId);
            return _mapper.Map<IEnumerable<FamilyFileDto>>(files);
        }

        public async Task<IEnumerable<FamilyFileDto>> GetByTypeAsync(
            int familyId,
            int? memberId,
            string fileType)
        {
            var files = await _repository.GetByTypeAsync(familyId, memberId, fileType);
            return _mapper.Map<IEnumerable<FamilyFileDto>>(files);
        }

        public async Task<FamilyFileDto?> GetByIdAsync(Guid fileId)
        {
            var file = await _repository.GetByIdAsync(fileId);
            return file == null ? null : _mapper.Map<FamilyFileDto>(file);
        }

        public async Task<FamilyFileDto> AddAsync(FamilyFileCreateDto createDto)
        {
            _logger.LogInformation("Adding family file for FamilyId: {FamilyId}", createDto.FamilyId);

            var entity = _mapper.Map<FamilyFile>(createDto);
            entity.Status = "Pending";

            await _repository.AddAsync(entity);
            return _mapper.Map<FamilyFileDto>(entity);
        }

        public async Task ApproveAsync(Guid fileId)
        {
            _logger.LogInformation("Approving family file FileId: {FileId}", fileId);
            var file = await _repository.GetByIdAsync(fileId)
                       ?? throw new KeyNotFoundException("Family file not found");

            file.Status = "Approved";
            await _repository.UpdateAsync(file);
        }

        public async Task RejectAsync(Guid fileId)
        {
            _logger.LogInformation("Rejecting family file FileId: {FileId}", fileId);
            var file = await _repository.GetByIdAsync(fileId)
                       ?? throw new KeyNotFoundException("Family file not found");

            file.Status = "Rejected";
            await _repository.UpdateAsync(file);
        }

        public async Task DeleteAsync(Guid fileId)
        {
            _logger.LogInformation("Deleting family file FileId: {FileId}", fileId);
            await _repository.DeleteAsync(fileId);
        }
        public async Task<PresignUploadResponseDto> GenerateUploadUrlAsync(
    PresignUploadRequestDto request)
        {
            // 1. Validate ownership (family belongs to user)
            // TODO: plug your existing ownership validation

            // 2. Get parish & unit from DB (via family)
            var family = await _familyRepository.GetByIdAsync(request.FamilyId)
                ?? throw new Exception("Family not found");

            // 3. Build S3 file key
            var fileKey = BuildFileKey(
                family.ParishId,
                family.UnitId,
                request.FamilyId,
                request.MemberId,
                request.FileType,
                request.FileName
            );

            // 4. Generate signed upload URL
            var uploadUrl = await _storageService.GenerateUploadUrlAsync(
                fileKey,
                request.ContentType
            );

            return new PresignUploadResponseDto
            {
                UploadUrl = uploadUrl,
                FileKey = fileKey
            };
        }
        public async Task<PresignDownloadResponseDto> GenerateDownloadUrlAsync(Guid fileId)
        {
            _logger.LogInformation("Generating signed download URL for FileId: {FileId}", fileId);

            var file = await _repository.GetByIdAsync(fileId)
                ?? throw new KeyNotFoundException("Family file not found");

            // 🔐 TODO: Add family ownership / role validation here
            // Example:
            // if (!UserHasAccess(file.FamilyId)) throw new UnauthorizedAccessException();

            var signedUrl = await _storageService.GenerateDownloadUrlAsync(file.FileKey);

            return new PresignDownloadResponseDto
            {
                SignedUrl = signedUrl,
                ExpiryMinutes = 10
            };
        }


        private string BuildFileKey(
    int parishId,
    int unitId,
    int familyId,
    int? memberId,
    string fileType,
    string fileName)
        {
            if (memberId.HasValue)
            {
                return
                    $"parishes/parish_{parishId}/units/unit_{unitId}/families/family_{familyId}/members/member_{memberId}/{fileName}";
            }

            return
                $"parishes/parish_{parishId}/units/unit_{unitId}/families/family_{familyId}/family/{fileName}";
        }

    }
}
