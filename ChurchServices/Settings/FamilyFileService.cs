using AutoMapper;
using ChurchContracts;
using ChurchData.Entities;
using ChurchDTOs.DTOs.Entities;
using ChurchServices.Storage;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Settings
{
    public class FamilyFileService : IFamilyFileService
    {
        private readonly IFamilyFileRepository _repository;
        private readonly ILogger<FamilyFileService> _logger;
        private readonly IMapper _mapper;
        private readonly IFamilyRepository _familyRepository;
        private readonly IFileStorageService _storageService;
        private const int MaxFilesPerFamily = 12;

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

            // Validate max files per family
            var existingFiles = await _repository.GetByFamilyAsync(createDto.FamilyId);
            if (existingFiles.Count() >= MaxFilesPerFamily)
            {
                throw new InvalidOperationException($"Family cannot have more than {MaxFilesPerFamily} files.");
            }

            var entity = _mapper.Map<FamilyFile>(createDto);
            entity.Status = "Approved";

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
            var family = await _familyRepository.GetByIdAsync(request.FamilyId)
                ?? throw new Exception("Family not found");

            var fileKey = BuildFileKey(
                family.ParishId,
                family.UnitId,
                request.FamilyId,
                request.MemberId,
                request.FileType,
                request.FileName
            );

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

            var signedUrl = await _storageService.GenerateDownloadUrlAsync(file.FileKey);

            return new PresignDownloadResponseDto
            {
                SignedUrl = signedUrl,
                ExpiryMinutes = 10
            };
        }

        public async Task<BulkPresignDownloadResponseDto> GenerateBulkDownloadUrlsAsync(int familyId, int? memberId = null)
        {
            _logger.LogInformation("Generating bulk signed download URLs for FamilyId: {FamilyId}, MemberId: {MemberId}", familyId, memberId);

            IEnumerable<FamilyFile> files;
            if (memberId.HasValue)
            {
                files = await _repository.GetByMemberAsync(familyId, memberId.Value);
            }
            else
            {
                files = await _repository.GetByFamilyAsync(familyId);
            }

            var fileSignedUrls = new List<FileSignedUrlDto>();

            foreach (var file in files)
            {
                var signedUrl = await _storageService.GenerateDownloadUrlAsync(file.FileKey);
                fileSignedUrls.Add(new FileSignedUrlDto
                {
                    FileId = file.FileId,
                    FileName = Path.GetFileName(file.FileKey),
                    FileType = file.FileType,
                    SignedUrl = signedUrl,
                    UploadedAt = file.UploadedAt
                });
            }

            return new BulkPresignDownloadResponseDto
            {
                Files = fileSignedUrls,
                ExpiryMinutes = 10
            };
        }

        public async Task<FamilyFileDto> UpdateAsync(Guid fileId, FamilyFileUpdateDto updateDto)
        {
            _logger.LogInformation("Updating family file FileId: {FileId}", fileId);

            var file = await _repository.GetByIdAsync(fileId)
                ?? throw new KeyNotFoundException("Family file not found");

            // If setting IsPrimary to true, unset other primary files for this family
            if (updateDto.IsPrimary.HasValue && updateDto.IsPrimary.Value)
            {
                var familyFiles = await _repository.GetByFamilyAsync(file.FamilyId);
                foreach (var existingFile in familyFiles.Where(f => f.FileId != fileId && f.IsPrimary))
                {
                    existingFile.IsPrimary = false;
                    await _repository.UpdateAsync(existingFile);
                }
            }

            file.FileType = updateDto.FileType;
            if (updateDto.FileCategory != null)
            {
                file.FileCategory = updateDto.FileCategory;
            }
            if (updateDto.IsPrimary.HasValue)
            {
                file.IsPrimary = updateDto.IsPrimary.Value;
            }

            // Save MemberId when provided in update
            if (updateDto.MemberId.HasValue)
            {
                file.MemberId = updateDto.MemberId;
            }

            await _repository.UpdateAsync(file);
            return _mapper.Map<FamilyFileDto>(file);
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
