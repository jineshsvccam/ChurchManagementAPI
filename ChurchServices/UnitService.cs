using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.Extensions.Logging;

namespace ChurchServices
{
    public class UnitService : IUnitService
    {
        private readonly IUnitRepository _unitRepository;
        private readonly ILogger<UnitService> _logger;
        private readonly IMapper _mapper;

        public UnitService(IUnitRepository unitRepository, ILogger<UnitService> logger, IMapper mapper)
        {
            _unitRepository = unitRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UnitDto>> GetAllAsync(int? parishId)
        {
            _logger.LogInformation("Fetching all units for parishId: {ParishId}", parishId);
            var units = await _unitRepository.GetAllAsync(parishId);
            var unitsDto = _mapper.Map<IEnumerable<UnitDto>>(units);
            _logger.LogInformation("Fetched {Count} units", unitsDto.Count());
            return unitsDto;
        }

        public async Task<UnitDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching unit by id: {Id}", id);
            var unit = await _unitRepository.GetByIdAsync(id);
            if (unit == null)
            {
                _logger.LogWarning("No unit found with id: {Id}", id);
                return null;
            }
            return _mapper.Map<UnitDto>(unit);
        }

        public async Task<UnitDto> AddAsync(UnitDto unitDto)
        {
            _logger.LogInformation("Adding new unit with name: {UnitName}", unitDto.UnitName);
            var unitEntity = _mapper.Map<Unit>(unitDto);
            var createdUnit = await _unitRepository.AddAsync(unitEntity);
            _logger.LogInformation("Created unit with id: {UnitId}", createdUnit.UnitId);
            return _mapper.Map<UnitDto>(createdUnit);
        }

        public async Task<UnitDto> UpdateAsync(UnitDto unitDto)
        {
            _logger.LogInformation("Updating unit with id: {UnitId}", unitDto.UnitId);
            var unitEntity = _mapper.Map<Unit>(unitDto);
            await _unitRepository.UpdateAsync(unitEntity);
            _logger.LogInformation("Updated unit with id: {UnitId}", unitDto.UnitId);
            return unitDto;
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting unit with id: {UnitId}", id);
            await _unitRepository.DeleteAsync(id);
            _logger.LogInformation("Deleted unit with id: {UnitId}", id);
        }

        public async Task<IEnumerable<UnitDto>> AddOrUpdateAsync(IEnumerable<UnitDto> requests)
        {
            _logger.LogInformation("Processing {Count} unit requests", requests.Count());
            var processedUnits = new List<Unit>();

            foreach (var request in requests)
            {
                var unitEntity = _mapper.Map<Unit>(request);
                if (request.Action == "INSERT")
                {
                    _logger.LogInformation("Inserting unit with name: {UnitName}", unitEntity.UnitName);
                    var createdUnit = await _unitRepository.AddAsync(unitEntity);
                    processedUnits.Add(createdUnit);
                }
                else if (request.Action == "UPDATE")
                {
                    _logger.LogInformation("Updating unit with id: {UnitId}", unitEntity.UnitId);
                    var updatedUnit = await _unitRepository.UpdateAsync(unitEntity);
                    processedUnits.Add(updatedUnit);
                }
                else
                {
                    _logger.LogError("Invalid action specified: {Action}", request.Action);
                    throw new ArgumentException("Invalid action specified");
                }
            }

            _logger.LogInformation("Processed {Count} unit requests", processedUnits.Count);
            return _mapper.Map<IEnumerable<UnitDto>>(processedUnits);
        }
    }
}
