using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchRepositories.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Settings
{
    public class FamilyService : IFamilyService
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly ILogger<FamilyService> _logger;
        private readonly IMapper _mapper;
        private readonly IFamilyQueryRepository _familyQueryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public FamilyService(IFamilyRepository familyRepository, ILogger<FamilyService> logger, IMapper mapper, IFamilyQueryRepository familyQueryRepository,
            IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _familyRepository = familyRepository ?? throw new ArgumentNullException(nameof(familyRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _familyQueryRepository = familyQueryRepository ?? throw new ArgumentNullException(nameof(familyQueryRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<FamilyDto>> GetFamiliesAsync(int? parishId, int? unitId, int? familyId)
        {
            _logger.LogInformation("Fetching families for ParishId: {ParishId}, UnitId: {UnitId}, FamilyId: {FamilyId}", parishId, unitId, familyId);

            var familiesWithPhoto = await _familyQueryRepository.GetFamiliesWithPhotoAsync(parishId, unitId, familyId);

            var result = familiesWithPhoto.Select(x =>
            {
                var dto = _mapper.Map<FamilyDto>(x.Family);
                dto.HasFamilyPhoto = x.HasFamilyPhoto;
                dto.FamilyPhotoFileId = x.FamilyPhotoFileId;
                return dto;
            }).ToList();

            return result;
        }

        public async Task<FamilyDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching family by Id: {Id}", id);
            var family = await _familyRepository.GetByIdAsync(id);
            if (family != null)
            {
                await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, family.ParishId);
            }
            return _mapper.Map<FamilyDto?>(family);
        }

        public async Task<IEnumerable<FamilyDto>> AddOrUpdateAsync(IEnumerable<FamilyDto> requests)
        {
            // Validate parish ownership for all DTOs in bulk request
            await ValidateBulkParishOwnershipAsync(requests);

            var processedFamilies = new List<FamilyDto>();

            foreach (var request in requests)
            {
                if (string.Equals(request.Action, "INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    var createdFamily = await AddAsync(request);
                    processedFamilies.Add(createdFamily);
                }
                else if (string.Equals(request.Action, "UPDATE", StringComparison.OrdinalIgnoreCase))
                {
                    var updatedFamily = await UpdateAsync(request);
                    processedFamilies.Add(updatedFamily);
                }
                else
                {
                    _logger.LogWarning("Invalid action specified: {Action}", request.Action);
                    throw new ArgumentException($"Invalid action specified: {request.Action}");
                }
            }
            return processedFamilies;
        }

        public async Task<FamilyDto> AddAsync(FamilyDto familyDto)
        {
            ValidateGeoLocation(familyDto);

            var familyEntity = _mapper.Map<Family>(familyDto);
            var addedFamily = await _familyRepository.AddAsync(familyEntity);
            _logger.LogInformation("Added new family with Id: {FamilyId}", addedFamily.FamilyId);
            return _mapper.Map<FamilyDto>(addedFamily);
        }

        public async Task<FamilyDto> UpdateAsync(FamilyDto familyDto)
        {
            var existingFamily = await _familyRepository.GetByIdAsync(familyDto.FamilyId);
            if (existingFamily == null)
            {
                throw new KeyNotFoundException("Family not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, existingFamily.ParishId);

            ValidateGeoLocation(familyDto);

            var familyEntity = _mapper.Map<Family>(familyDto);
            if (familyEntity.JoinDate.HasValue)
            {
                familyEntity.JoinDate = DateTime.SpecifyKind(familyEntity.JoinDate.Value, DateTimeKind.Utc);
            }
            var updatedFamily = await _familyRepository.UpdateAsync(familyEntity);
            _logger.LogInformation("Updated family with Id: {FamilyId}", updatedFamily.FamilyId);
            return _mapper.Map<FamilyDto>(updatedFamily);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting family with Id: {Id}", id);
            await _familyRepository.DeleteAsync(id);
        }

        private static void ValidateGeoLocation(FamilyDto familyDto)
        {
            if (familyDto.Latitude.HasValue && (familyDto.Latitude < -90 || familyDto.Latitude > 90))
                throw new ArgumentException("Latitude must be between -90 and 90");

            if (familyDto.Longitude.HasValue && (familyDto.Longitude < -180 || familyDto.Longitude > 180))
                throw new ArgumentException("Longitude must be between -180 and 180");
        }

        private async Task ValidateBulkParishOwnershipAsync<TDto>(IEnumerable<TDto> requests) where TDto : class
        {
            var (roleName, userParishId) = await UserHelper.GetCurrentUserRoleAndParishAsync(_httpContextAccessor, _context);

            // Admin users can modify any parish data
            if (string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (var request in requests)
            {
                if (request is ChurchDTOs.DTOs.Utils.IParishEntity parishEntity)
                {
                    if (userParishId == null || parishEntity.ParishId != userParishId)
                    {
                        throw new UnauthorizedAccessException("You are not authorized to modify data from another parish.");
                    }
                }
            }
        }
    }
}
