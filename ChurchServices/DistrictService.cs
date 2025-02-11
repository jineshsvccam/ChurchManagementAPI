using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using Microsoft.Extensions.Logging;

namespace ChurchServices
{
    public class DistrictService : IDistrictService
    {
        private readonly IDistrictRepository _districtRepository;
        private readonly ILogger<DistrictService> _logger;

        public DistrictService(IDistrictRepository districtRepository, ILogger<DistrictService> logger)
        {
            _districtRepository = districtRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<District>> GetAllAsync()
        {
            try
            {
                return await _districtRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all districts.");
                throw;
            }
        }

        public async Task<District> GetByIdAsync(int id)
        {
            try
            {
                var district = await _districtRepository.GetByIdAsync(id);

                if (district == null)
                {
                    _logger.LogWarning("No district found with ID: {Id}", id);
                }
                return district;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching district with ID: {Id}", id);
                throw;
            }
        }

        public async Task AddAsync(District district)
        {
            try
            {
                await _districtRepository.AddAsync(district);
                _logger.LogInformation("Added district: {Name}", district.DistrictName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding district: {Name}", district.DistrictName);
                throw;
            }
        }

        public async Task UpdateAsync(District district)
        {
            try
            {
                await _districtRepository.UpdateAsync(district);
                _logger.LogInformation("Updated district with ID: {Id}", district.DistrictId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating district with ID: {Id}", district.DistrictId);
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
