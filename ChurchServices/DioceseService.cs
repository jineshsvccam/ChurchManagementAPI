using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using Microsoft.Extensions.Logging;

namespace ChurchServices
{
    public class DioceseService : IDioceseService
    {
        private readonly IDioceseRepository _dioceseRepository;
        private readonly ILogger<DioceseService> _logger;

        public DioceseService(IDioceseRepository dioceseRepository, ILogger<DioceseService> logger)
        {
            _dioceseRepository = dioceseRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Diocese>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all dioceses from service layer.");
            return await _dioceseRepository.GetAllAsync();
        }

        public async Task<Diocese> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching diocese with ID: {Id} from service layer.", id);
            return await _dioceseRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Diocese diocese)
        {
            _logger.LogInformation("Adding new diocese: {@Diocese}", diocese);
            await _dioceseRepository.AddAsync(diocese);
            _logger.LogInformation("Diocese added successfully with ID: {Id}", diocese.DioceseId);
        }

        public async Task UpdateAsync(Diocese diocese)
        {
            _logger.LogInformation("Updating diocese with ID: {Id}", diocese.DioceseId);
            await _dioceseRepository.UpdateAsync(diocese);
            _logger.LogInformation("Diocese with ID {Id} updated successfully.", diocese.DioceseId);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting diocese with ID: {Id}", id);
            await _dioceseRepository.DeleteAsync(id);
            _logger.LogInformation("Diocese with ID {Id} deleted successfully.", id);
        }
    }
}
