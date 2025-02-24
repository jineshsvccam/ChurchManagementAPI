using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchRepositories;

namespace ChurchServices
{
    public class ContributionSettingsService : IContributionSettingsService
    {
        private readonly IContributionSettingsRepository _repository;
        private readonly IMapper _mapper;

        public ContributionSettingsService(IContributionSettingsRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ContributionSettingsDto>> GetAllAsync(int? parishId)
        {
            var settings = await _repository.GetAllAsync(parishId);
            return _mapper.Map<IEnumerable<ContributionSettingsDto>>(settings);
        }

        public async Task<ContributionSettingsDto?> GetByIdAsync(int settingId)
        {
            var setting = await _repository.GetByIdAsync(settingId);
            return setting is not null ? _mapper.Map<ContributionSettingsDto>(setting) : null;
        }

        public async Task<ContributionSettingsDto> AddAsync(ContributionSettingsDto contributionSettingsDto)
        {
            var entity = _mapper.Map<ContributionSettings>(contributionSettingsDto);
            await _repository.AddAsync(entity);
            return _mapper.Map<ContributionSettingsDto>(entity);
        }

        public async Task<ContributionSettingsDto> UpdateAsync(int settingId, ContributionSettingsDto contributionSettingsDto)
        {
            var existing = await _repository.GetByIdAsync(settingId);
            if (existing == null)
            {

                throw new KeyNotFoundException("Contribution settings not found.");
            }
            var contibutionsetting = _mapper.Map<ContributionSettings>(contributionSettingsDto);           
            await _repository.UpdateAsync(contibutionsetting);
            return _mapper.Map<ContributionSettingsDto>(contibutionsetting);
        }

        public async Task DeleteAsync(int settingId)
        {
            await _repository.DeleteAsync(settingId);
        }
        public async Task<IEnumerable<ContributionSettingsDto>> AddOrUpdateAsync(IEnumerable<ContributionSettingsDto> requests)
        {
            var processedEntries = new List<ContributionSettings>();

            foreach (var request in requests)
            {
                // Map DTO to entity
                var settingEntity = _mapper.Map<ContributionSettings>(request);
                if (request.Action == "INSERT")
                {
                    var createdSettings = await _repository.AddAsync(settingEntity);
                    processedEntries.Add(createdSettings);
                }
                else if (request.Action == "UPDATE")
                {
                    var updatedFamily = await _repository.UpdateAsync(settingEntity);
                    processedEntries.Add(updatedFamily);
                }
                else
                {
                    throw new ArgumentException("Invalid action specified");
                }
            }
            return _mapper.Map<IEnumerable<ContributionSettingsDto>>(processedEntries);
        }

    }

}
