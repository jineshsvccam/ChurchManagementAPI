using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Admin
{
    public class FinancialYearService : IFinancialYearService
    {
        private readonly IFinancialYearRepository _financialYearRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<FinancialYearService> _logger;

        public FinancialYearService(
            IFinancialYearRepository financialYearRepository,
            IMapper mapper,
            ILogger<FinancialYearService> logger)
        {
            _financialYearRepository = financialYearRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<FinancialYearDto?> GetFinancialYearByDateAsync(int parishId, DateTime date)
        {
            var entity = await _financialYearRepository.GetFinancialYearByDateAsync(parishId, date);
            return _mapper.Map<FinancialYearDto?>(entity);
        }

        public async Task<FinancialYearDto?> GetByIdAsync(int financialYearId)
        {
            var entity = await _financialYearRepository.GetByIdAsync(financialYearId);
            return _mapper.Map<FinancialYearDto?>(entity);
        }

        public async Task<FinancialYearDto> AddAsync(FinancialYearDto financialYearDto)
        {
            var entity = _mapper.Map<FinancialYear>(financialYearDto);
            var result = await _financialYearRepository.AddAsync(entity);
            return _mapper.Map<FinancialYearDto>(result);
        }

        public async Task<FinancialYearDto> UpdateAsync(FinancialYearDto financialYearDto)
        {
            var entity = _mapper.Map<FinancialYear>(financialYearDto);
            var result = await _financialYearRepository.UpdateAsync(entity);
            return _mapper.Map<FinancialYearDto>(result);
        }

        public async Task DeleteAsync(int financialYearId)
        {
            _logger.LogInformation($"Deleting financial year with ID {financialYearId}");
            await _financialYearRepository.DeleteAsync(financialYearId);
        }

        public async Task<IEnumerable<FinancialYearDto>> GetAllAsync(int? parishId)
        {
            var entities = await _financialYearRepository.GetAllAsync(parishId);
            return _mapper.Map<IEnumerable<FinancialYearDto>>(entities);
        }

        public async Task LockFinancialYearAsync(int financialYearId)
        {
            var financialYear = await _financialYearRepository.GetByIdAsync(financialYearId);
            if (financialYear == null)
            {
                _logger.LogWarning($"Financial year with ID {financialYearId} not found.");
                throw new InvalidOperationException("Financial year not found.");
            }

            financialYear.IsLocked = true;
            financialYear.LockDate = DateTime.UtcNow;

            await _financialYearRepository.UpdateAsync(financialYear);
            _logger.LogInformation($"Financial year {financialYearId} has been locked.");
        }
    }
}
