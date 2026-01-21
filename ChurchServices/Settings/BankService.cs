using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts.ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Settings
{
    public class BankService : IBankService
    {
        private readonly IBankRepository _bankRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BankService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public BankService(IBankRepository bankRepository, IMapper mapper, ILogger<BankService> logger,
            IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _bankRepository = bankRepository;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<IEnumerable<BankDto>> GetBanksAsync(int? parishId, int? bankId)
        {
            _logger.LogInformation("Fetching banks for ParishId: {ParishId}, BankId: {BankId}", parishId, bankId);
            var banks = await _bankRepository.GetBanksAsync(parishId, bankId);
            return _mapper.Map<IEnumerable<BankDto>>(banks);
        }

        public async Task<BankDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching bank by Id: {Id}", id);
            var bank = await _bankRepository.GetByIdAsync(id);
            if (bank != null)
            {
                await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, bank.ParishId);
            }
            return _mapper.Map<BankDto?>(bank);
        }

        public async Task<IEnumerable<BankDto>> AddOrUpdateAsync(IEnumerable<BankDto> requests)
        {
            // Validate parish ownership for all DTOs in bulk request
            await ValidateBulkParishOwnershipAsync(requests);

            var createdBanks = new List<BankDto>();

            foreach (var request in requests)
            {
                if (string.Equals(request.Action, "INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    var createdBank = await AddAsync(request);
                    createdBanks.Add(createdBank);
                }
                else if (string.Equals(request.Action, "UPDATE", StringComparison.OrdinalIgnoreCase))
                {
                    var updatedBank = await UpdateAsync(request);
                    createdBanks.Add(updatedBank);
                }
                else
                {
                    _logger.LogWarning("Invalid action specified: {Action}", request.Action);
                    throw new ArgumentException($"Invalid action specified: {request.Action}");
                }
            }
            return createdBanks;
        }

        public async Task<BankDto> AddAsync(BankDto bankDto)
        {
            var bank = _mapper.Map<Bank>(bankDto);
            var addedBank = await _bankRepository.AddAsync(bank);
            _logger.LogInformation("Added new bank with Id: {BankId}", addedBank.BankId);
            return _mapper.Map<BankDto>(addedBank);
        }

        public async Task<BankDto> UpdateAsync(BankDto bankDto)
        {
            var existingBank = await _bankRepository.GetByIdAsync(bankDto.BankId);
            if (existingBank == null)
            {
                throw new KeyNotFoundException("Bank not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, existingBank.ParishId);

            var bank = _mapper.Map<Bank>(bankDto);
            var updatedBank = await _bankRepository.UpdateAsync(bank);
            _logger.LogInformation("Updated bank with Id: {BankId}", updatedBank.BankId);
            return _mapper.Map<BankDto>(updatedBank);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting bank with Id: {Id}", id);
            await _bankRepository.DeleteAsync(id);
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
