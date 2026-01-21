using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Settings
{
    public class ContributionSettingsService : IContributionSettingsService
    {
        private readonly IContributionSettingsRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ContributionSettingsService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public ContributionSettingsService(IContributionSettingsRepository repository, IMapper mapper, ILogger<ContributionSettingsService> logger,
            IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<ContributionSettingsDto>> GetAllAsync(int? parishId)
        {
            _logger.LogInformation("Fetching contribution settings for ParishId: {ParishId}", parishId);
            var settings = await _repository.GetAllAsync(parishId);
            return _mapper.Map<IEnumerable<ContributionSettingsDto>>(settings);
        }

        public async Task<ContributionSettingsDto?> GetByIdAsync(int settingId)
        {
            _logger.LogInformation("Fetching contribution setting by Id: {SettingId}", settingId);
            var setting = await _repository.GetByIdAsync(settingId);
            if (setting != null)
            {
                await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, setting.ParishId);
            }
            return _mapper.Map<ContributionSettingsDto?>(setting);
        }

        public async Task<IEnumerable<ContributionSettingsDto>> AddOrUpdateAsync(IEnumerable<ContributionSettingsDto> requests)
        {
            // Validate parish ownership for all DTOs in bulk request
            await ValidateBulkParishOwnershipAsync(requests);

            var processedSettings = new List<ContributionSettingsDto>();

            foreach (var request in requests)
            {
                if (string.Equals(request.Action, "INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    var createdSetting = await AddAsync(request);
                    processedSettings.Add(createdSetting);
                }
                else if (string.Equals(request.Action, "UPDATE", StringComparison.OrdinalIgnoreCase))
                {
                    var updatedSetting = await UpdateAsync(request.SettingId, request);
                    processedSettings.Add(updatedSetting);
                }
                else
                {
                    _logger.LogWarning("Invalid action specified: {Action}", request.Action);
                    throw new ArgumentException($"Invalid action specified: {request.Action}");
                }
            }
            return processedSettings;
        }

        public async Task<ContributionSettingsDto> AddAsync(ContributionSettingsDto contributionSettingsDto)
        {
            contributionSettingsDto.ValidFrom = DateTime.SpecifyKind(contributionSettingsDto.ValidFrom, DateTimeKind.Utc);
            var entity = _mapper.Map<ContributionSettings>(contributionSettingsDto);
            var addedEntity = await _repository.AddAsync(entity);
            _logger.LogInformation("Added new contribution setting with Id: {SettingId}", addedEntity.SettingId);
            return _mapper.Map<ContributionSettingsDto>(addedEntity);
        }

        public async Task<ContributionSettingsDto> UpdateAsync(int settingId, ContributionSettingsDto contributionSettingsDto)
        {
            var existingEntity = await _repository.GetByIdAsync(settingId);
            if (existingEntity == null)
            {
                throw new KeyNotFoundException("Contribution setting not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, existingEntity.ParishId);

            contributionSettingsDto.ValidFrom = DateTime.SpecifyKind(contributionSettingsDto.ValidFrom, DateTimeKind.Utc);
            var entity = _mapper.Map<ContributionSettings>(contributionSettingsDto);
            var updatedEntity = await _repository.UpdateAsync(entity);
            _logger.LogInformation("Updated contribution setting with Id: {SettingId}", updatedEntity.SettingId);
            return _mapper.Map<ContributionSettingsDto>(updatedEntity);
        }

        public async Task DeleteAsync(int settingId)
        {
            _logger.LogInformation("Deleting contribution setting with Id: {SettingId}", settingId);
            await _repository.DeleteAsync(settingId);
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
