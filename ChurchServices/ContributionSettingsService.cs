using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<ContributionSettingsDto>> GetAllAsync()
        {
            var settings = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<ContributionSettingsDto>>(settings);
        }

        public async Task<ContributionSettingsDto?> GetByIdAsync(int settingId)
        {
            var setting = await _repository.GetByIdAsync(settingId);
            return setting is not null ? _mapper.Map<ContributionSettingsDto>(setting) : null;
        }

        public async Task<ContributionSettings> AddAsync(ContributionSettingsDto contributionSettingsDto)
        {
            var entity = _mapper.Map<ContributionSettings>(contributionSettingsDto);
            return await _repository.AddAsync(entity);
        }

        public async Task<ContributionSettings> UpdateAsync(int settingId, ContributionSettingsDto contributionSettingsDto)
        {
            var existing = await _repository.GetByIdAsync(settingId);
            if (existing == null)
            {
                
                throw new KeyNotFoundException("Contribution settings not found.");
            }
            var entity = _mapper.Map(contributionSettingsDto, existing);
            return await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int settingId)
        {
            await _repository.DeleteAsync(settingId);
        }
    }

}
