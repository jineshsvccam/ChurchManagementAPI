using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.Extensions.Logging;

namespace ChurchServices
{
    public class DioceseService : IDioceseService
    {
        private readonly IDioceseRepository _dioceseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DioceseService> _logger;

        public DioceseService(IDioceseRepository dioceseRepository, IMapper mapper, ILogger<DioceseService> logger)
        {
            _dioceseRepository = dioceseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DioceseDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all dioceses from service layer.");
            var dioceses = await _dioceseRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DioceseDto>>(dioceses);
        }

        public async Task<DioceseDto> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching diocese with ID: {Id} from service layer.", id);
            var diocese = await _dioceseRepository.GetByIdAsync(id);
            return _mapper.Map<DioceseDto>(diocese);
        }

        public async Task AddAsync(DioceseDto dioceseDto)
        {
            _logger.LogInformation("Adding new diocese: {@DioceseDto}", dioceseDto);
            var diocese = _mapper.Map<Diocese>(dioceseDto);
            await _dioceseRepository.AddAsync(diocese);
            _logger.LogInformation("Diocese added successfully with ID: {Id}", diocese.DioceseId);
        }

        public async Task UpdateAsync(DioceseDto dioceseDto)
        {
            _logger.LogInformation("Updating diocese with ID: {Id}", dioceseDto.DioceseId);
            var diocese = _mapper.Map<Diocese>(dioceseDto);
            await _dioceseRepository.UpdateAsync(diocese);
            _logger.LogInformation("Diocese with ID {Id} updated successfully.", dioceseDto.DioceseId);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting diocese with ID: {Id}", id);
            await _dioceseRepository.DeleteAsync(id);
            _logger.LogInformation("Diocese with ID {Id} deleted successfully.", id);
        }
    }
}
