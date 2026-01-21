using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Admin
{
    public class DistrictService : IDistrictService
    {
        private readonly IDistrictRepository _districtRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DistrictService> _logger;

        public DistrictService(IDistrictRepository districtRepository, IMapper mapper, ILogger<DistrictService> logger)
        {
            _districtRepository = districtRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DistrictDto>> GetAllAsync()
        {
            try
            {
                var districts = await _districtRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<DistrictDto>>(districts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all districts.");
                throw;
            }
        }

        public async Task<DistrictDto> GetByIdAsync(int id)
        {
            try
            {
                var district = await _districtRepository.GetByIdAsync(id);
                if (district == null)
                {
                    _logger.LogWarning("No district found with ID: {Id}", id);
                    return null;
                }
                return _mapper.Map<DistrictDto>(district);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching district with ID: {Id}", id);
                throw;
            }
        }

        public async Task<DistrictDto> AddAsync(DistrictDto districtDto)
        {
            try
            {
                var district = _mapper.Map<District>(districtDto);
                await _districtRepository.AddAsync(district);
                _logger.LogInformation("Added district: {Name}", district.DistrictName);
                return _mapper.Map<DistrictDto>(district);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding district: {Name}", districtDto.DistrictName);
                throw;
            }
        }

        public async Task<DistrictDto> UpdateAsync(DistrictDto districtDto)
        {
            try
            {
                var district = _mapper.Map<District>(districtDto);
                await _districtRepository.UpdateAsync(district);
                _logger.LogInformation("Updated district with ID: {Id}", district.DistrictId);
                return _mapper.Map<DistrictDto>(district);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating district with ID: {Id}", districtDto.DistrictId);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await _districtRepository.DeleteAsync(id);
                _logger.LogInformation("Deleted district with ID: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting district with ID: {Id}", id);
                throw;
            }
        }
    }
}
