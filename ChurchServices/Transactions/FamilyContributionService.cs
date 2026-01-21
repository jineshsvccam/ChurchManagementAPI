using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Transactions
{
    public class FamilyContributionService : IFamilyContributionService
    {
        private readonly IFamilyContributionRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FamilyContributionService> _logger;
        private readonly ApplicationDbContext _context;

        public FamilyContributionService(IFamilyContributionRepository repository,
            IMapper mapper, IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<FamilyContributionService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<FamilyContributionDto>> GetAllAsync(int? parishId)
        {
            _logger.LogInformation("Fetching family contributions for ParishId: {ParishId}", parishId);
            var contributions = await _repository.GetAllAsync(parishId);
            return _mapper.Map<IEnumerable<FamilyContributionDto>>(contributions);
        }

        public async Task<FamilyContributionDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching family contribution by Id: {Id}", id);
            var contribution = await _repository.GetByIdAsync(id);
            if (contribution != null)
            {
                await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, contribution.ParishId);
            }
            return _mapper.Map<FamilyContributionDto>(contribution);
        }

        public async Task<FamilyContributionDto> AddAsync(FamilyContributionDto familyContributionDto)
        {
            var entity = _mapper.Map<FamilyContribution>(familyContributionDto);
            entity.CreatedBy = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);

            var addedEntity = await _repository.AddAsync(entity);
            _logger.LogInformation("Added new family contribution with Id: {Id}", addedEntity.ContributionId);
            return _mapper.Map<FamilyContributionDto>(addedEntity);
        }

        public async Task<FamilyContributionDto> UpdateAsync(FamilyContributionDto familyContributionDto)
        {
            var existingContribution = await _repository.GetByIdAsync(familyContributionDto.ContributionId);
            if (existingContribution == null)
            {
                throw new InvalidOperationException("Family contribution not found.");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, existingContribution.ParishId);

            var originalCreatedBy = existingContribution.CreatedBy;

            var entity = _mapper.Map<FamilyContribution>(familyContributionDto);

            entity.UpdatedBy = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.CreatedBy = originalCreatedBy;

            var updatedEntity = await _repository.UpdateAsync(entity);
            _logger.LogInformation("Updated family contribution with Id: {Id}", updatedEntity.ContributionId);
            return _mapper.Map<FamilyContributionDto>(updatedEntity);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting family contribution with Id: {Id}", id);
            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<FamilyContributionDto>> AddOrUpdateAsync(IEnumerable<FamilyContributionDto> requests)
        {
            // Validate parish ownership for all DTOs in bulk request
            await ValidateBulkParishOwnershipAsync(requests);

            var processedEntries = new List<FamilyContributionDto>();

            foreach (var request in requests)
            {
                if (string.Equals(request.Action, "INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    var created = await AddAsync(request);
                    processedEntries.Add(created);
                }
                else if (string.Equals(request.Action, "UPDATE", StringComparison.OrdinalIgnoreCase))
                {
                    var updated = await UpdateAsync(request);
                    processedEntries.Add(updated);
                }
                else
                {
                    _logger.LogWarning("Invalid action specified: {Action}", request.Action);
                    throw new ArgumentException($"Invalid action specified: {request.Action}");
                }
            }
            return processedEntries;
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
