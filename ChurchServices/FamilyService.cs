using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.Extensions.Logging;

namespace ChurchServices
{
    public class FamilyService : IFamilyService
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly ILogger<FamilyService> _logger;
        private readonly IMapper _mapper;

        public FamilyService(IFamilyRepository familyRepository, ILogger<FamilyService> logger, IMapper mapper)
        {
            _familyRepository = familyRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FamilyDto>> GetFamiliesAsync(int? parishId, int? unitId, int? familyId)
        {
            _logger.LogInformation("Fetching families with ParishId: {ParishId}, UnitId: {UnitId}, FamilyId: {FamilyId}", parishId, unitId, familyId);
            var families = await _familyRepository.GetFamiliesAsync(parishId, unitId, familyId);
            var familiesDto = _mapper.Map<IEnumerable<FamilyDto>>(families);
            _logger.LogInformation("Fetched {Count} families", familiesDto.Count());
            return familiesDto;
        }

        public async Task<FamilyDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching family with Id: {Id}", id);
            var family = await _familyRepository.GetByIdAsync(id);
            if (family == null)
            {
                _logger.LogWarning("Family with Id: {Id} not found", id);
                return null;
            }
            return _mapper.Map<FamilyDto>(family);
        }

        public async Task<IEnumerable<FamilyDto>> AddOrUpdateAsync(IEnumerable<FamilyDto> requests)
        {
            _logger.LogInformation("Processing {Count} families for AddOrUpdate.", requests.Count());
            var processedFamilies = new List<Family>();

            foreach (var request in requests)
            {
                // Map DTO to entity
                var familyEntity = _mapper.Map<Family>(request);
                if (request.Action == "INSERT")
                {
                    _logger.LogInformation("Adding new family: {FamilyName}", familyEntity.FamilyName);
                    var createdFamily = await _familyRepository.AddAsync(familyEntity);
                    processedFamilies.Add(createdFamily);
                }
                else if (request.Action == "UPDATE")
                {
                    _logger.LogInformation("Updating family with Id: {Id}", familyEntity.FamilyId);
                    var updatedFamily = await _familyRepository.UpdateAsync(familyEntity);
                    processedFamilies.Add(updatedFamily);
                }
                else
                {
                    _logger.LogWarning("Invalid action '{Action}' specified for family Id: {Id}", request.Action, familyEntity.FamilyId);
                    throw new ArgumentException("Invalid action specified");
                }
            }

            _logger.LogInformation("Successfully processed {Count} families for AddOrUpdate.", processedFamilies.Count);
            return _mapper.Map<IEnumerable<FamilyDto>>(processedFamilies);
        }

        public async Task<FamilyDto> AddAsync(FamilyDto familyDto)
        {
            _logger.LogInformation("Adding family: {FamilyName}", familyDto.FamilyName);
            var familyEntity = _mapper.Map<Family>(familyDto);
            var addedFamily = await _familyRepository.AddAsync(familyEntity);
            _logger.LogInformation("Successfully added family Id: {Id}", addedFamily.FamilyId);
            return _mapper.Map<FamilyDto>(addedFamily);
        }

        public async Task<FamilyDto> UpdateAsync(FamilyDto familyDto)
        {
            _logger.LogInformation("Updating family with Id: {Id}", familyDto.FamilyId);
            var familyEntity = _mapper.Map<Family>(familyDto);
            if (familyEntity.JoinDate.HasValue)
            {
                familyEntity.JoinDate = DateTime.SpecifyKind(familyEntity.JoinDate.Value, DateTimeKind.Utc);
            }
            var updatedFamily = await _familyRepository.UpdateAsync(familyEntity);
            _logger.LogInformation("Successfully updated family Id: {Id}", updatedFamily.FamilyId);
            return _mapper.Map<FamilyDto>(updatedFamily);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting family with Id: {Id}", id);
            await _familyRepository.DeleteAsync(id);
            _logger.LogInformation("Successfully deleted family with Id: {Id}", id);
        }
    }
}
