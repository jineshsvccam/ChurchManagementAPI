using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;

using ChurchRepositories;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}
