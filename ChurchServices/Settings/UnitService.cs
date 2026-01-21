using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Settings
{
    public class UnitService : IUnitService
    {
        private readonly IUnitRepository _unitRepository;
        private readonly ILogger<UnitService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public UnitService(IUnitRepository unitRepository, ILogger<UnitService> logger, IMapper mapper,
            IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _unitRepository = unitRepository ?? throw new ArgumentNullException(nameof(unitRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<UnitDto>> GetAllAsync(int? parishId)
        {
            _logger.LogInformation("Fetching units for ParishId: {ParishId}", parishId);
            var units = await _unitRepository.GetAllAsync(parishId);
            return _mapper.Map<IEnumerable<UnitDto>>(units);
        }

        public async Task<UnitDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching unit by Id: {Id}", id);
            var unit = await _unitRepository.GetByIdAsync(id);
            if (unit != null)
            {
                await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, unit.ParishId);
            }
            return _mapper.Map<UnitDto?>(unit);
        }

        public async Task<IEnumerable<UnitDto>> AddOrUpdateAsync(IEnumerable<UnitDto> requests)
        {
            // Validate parish ownership for all DTOs in bulk request
            await ValidateBulkParishOwnershipAsync(requests);

            var processedUnits = new List<UnitDto>();

            foreach (var request in requests)
            {
                if (string.Equals(request.Action, "INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    var createdUnit = await AddAsync(request);
                    processedUnits.Add(createdUnit);
                }
                else if (string.Equals(request.Action, "UPDATE", StringComparison.OrdinalIgnoreCase))
                {
                    var updatedUnit = await UpdateAsync(request);
                    processedUnits.Add(updatedUnit);
                }
                else
                {
                    _logger.LogWarning("Invalid action specified: {Action}", request.Action);
                    throw new ArgumentException($"Invalid action specified: {request.Action}");
                }
            }
            return processedUnits;
        }

        public async Task<UnitDto> AddAsync(UnitDto unitDto)
        {
            var unit = _mapper.Map<Unit>(unitDto);
            var addedUnit = await _unitRepository.AddAsync(unit);
            _logger.LogInformation("Added new unit with Id: {UnitId}", addedUnit.UnitId);
            return _mapper.Map<UnitDto>(addedUnit);
        }

        public async Task<UnitDto> UpdateAsync(UnitDto unitDto)
        {
            var existingUnit = await _unitRepository.GetByIdAsync(unitDto.UnitId);
            if (existingUnit == null)
            {
                throw new KeyNotFoundException("Unit not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, existingUnit.ParishId);

            var unit = _mapper.Map<Unit>(unitDto);
            var updatedUnit = await _unitRepository.UpdateAsync(unit);
            _logger.LogInformation("Updated unit with Id: {UnitId}", updatedUnit.UnitId);
            return _mapper.Map<UnitDto>(updatedUnit);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting unit with Id: {Id}", id);
            await _unitRepository.DeleteAsync(id);
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
