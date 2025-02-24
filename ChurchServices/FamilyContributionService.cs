using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;

namespace ChurchServices
{
    public class FamilyContributionService : IFamilyContributionService
    {
        private readonly IFamilyContributionRepository _repository;
        private readonly IMapper _mapper;

        public FamilyContributionService(IFamilyContributionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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
            var addedEntity = await _repository.AddAsync(entity);
            return _mapper.Map<FamilyContributionDto>(addedEntity);
        }

        public async Task<FamilyContributionDto> UpdateAsync(FamilyContributionDto familyContributionDto)
        {
            var entity = _mapper.Map<FamilyContribution>(familyContributionDto);
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
