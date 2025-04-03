using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchServices
{
    public class FamilyContributionService : IFamilyContributionService
    {
        private readonly IFamilyContributionRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FamilyContributionService> _logger;
        private readonly ApplicationDbContext _context;

        public FamilyContributionService(IFamilyContributionRepository repository,
            IMapper mapper,IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<FamilyContributionService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _logger = logger;
            _context = context;
        }

        public async Task<IEnumerable<FamilyContributionDto>> GetAllAsync(int? parishId)
        {
            var contributions = await _repository.GetAllAsync(parishId);
            return _mapper.Map<IEnumerable<FamilyContributionDto>>(contributions);
        }

        public async Task<FamilyContributionDto?> GetByIdAsync(int id)
        {
            var contribution = await _repository.GetByIdAsync(id);
            return _mapper.Map<FamilyContributionDto>(contribution);
        }

        public async Task<FamilyContributionDto> AddAsync(FamilyContributionDto familyContributionDto)
        {
            var entity = _mapper.Map<FamilyContribution>(familyContributionDto);
            entity.CreatedBy = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);

            var addedEntity = await _repository.AddAsync(entity);
            return _mapper.Map<FamilyContributionDto>(addedEntity);
        }

        public async Task<FamilyContributionDto> UpdateAsync(FamilyContributionDto familyContributionDto)
        {
            var (_, userParishId, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);

            var existingTransaction = await _repository.GetByIdAsync(familyContributionDto.ContributionId);
            if (existingTransaction == null)
            {
                throw new InvalidOperationException("Transaction not found.");
            }

            if (existingTransaction.ParishId != userParishId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this transaction.");
            }

            // Store the original CreatedBy value
            var originalCreatedBy = existingTransaction.CreatedBy;

            var entity = _mapper.Map<FamilyContribution>(familyContributionDto);

            entity.UpdatedBy = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.CreatedBy = originalCreatedBy;

            var updatedEntity = await _repository.UpdateAsync(entity);
            return _mapper.Map<FamilyContributionDto>(updatedEntity);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
        public async Task<IEnumerable<FamilyContributionDto>> AddOrUpdateAsync(IEnumerable<FamilyContributionDto> requests)
        {
            var processedEntries = new List<FamilyContribution>();

            foreach (var request in requests)
            {
                // Map DTO to entity
                var familyEntity = _mapper.Map<FamilyContribution>(request);
                if (request.Action == "INSERT")
                {
                    var createdFamily = await _repository.AddAsync(familyEntity);
                    processedEntries.Add(createdFamily);
                }
                else if (request.Action == "UPDATE")
                {
                    var updatedFamily = await _repository.UpdateAsync(familyEntity);
                    processedEntries.Add(updatedFamily);
                }
                else
                {
                    throw new ArgumentException("Invalid action specified");
                }
            }
            return _mapper.Map<IEnumerable<FamilyContributionDto>>(processedEntries);
        }


    }
}
