using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Settings
{
    public class FamilyDueService : IFamilyDueService
    {
        private readonly IFamilyDueRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<FamilyDueService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public FamilyDueService(IFamilyDueRepository repository, IMapper mapper, ILogger<FamilyDueService> logger,
            IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<FamilyDueDto>> GetAllAsync(int? parishId)
        {
            _logger.LogInformation("Fetching family dues for ParishId: {ParishId}", parishId);
            var dues = await _repository.GetAllAsync(parishId);
            return _mapper.Map<IEnumerable<FamilyDueDto>>(dues);
        }

        public async Task<FamilyDueDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching family due by Id: {Id}", id);
            var due = await _repository.GetByIdAsync(id);
            if (due != null)
            {
                await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, due.ParishId);
            }
            return _mapper.Map<FamilyDueDto?>(due);
        }

        public async Task<IEnumerable<FamilyDueDto>> AddOrUpdateAsync(IEnumerable<FamilyDueDto> requests)
        {
            // Validate parish ownership for all DTOs in bulk request
            await ValidateBulkParishOwnershipAsync(requests);

            var processedDues = new List<FamilyDueDto>();

            foreach (var request in requests)
            {
                if (string.Equals(request.Action, "INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    var createdDue = await AddAsync(request);
                    processedDues.Add(createdDue);
                }
                else if (string.Equals(request.Action, "UPDATE", StringComparison.OrdinalIgnoreCase))
                {
                    var updatedDue = await UpdateAsync(request.DuesId, request);
                    processedDues.Add(updatedDue);
                }
                else
                {
                    _logger.LogWarning("Invalid action specified: {Action}", request.Action);
                    throw new ArgumentException($"Invalid action specified: {request.Action}");
                }
            }
            return processedDues;
        }

        public async Task<FamilyDueDto> AddAsync(FamilyDueDto dto)
        {
            var due = _mapper.Map<FamilyDue>(dto);
            var addedDue = await _repository.AddAsync(due);
            _logger.LogInformation("Added new family due with Id: {DuesId}", addedDue.DuesId);
            return _mapper.Map<FamilyDueDto>(addedDue);
        }

        public async Task<FamilyDueDto> UpdateAsync(int id, FamilyDueDto dto)
        {
            var existingDue = await _repository.GetByIdAsync(id);
            if (existingDue == null)
            {
                throw new KeyNotFoundException("Family due not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, existingDue.ParishId);

            var due = _mapper.Map<FamilyDue>(dto);
            var updatedDue = await _repository.UpdateAsync(due);
            _logger.LogInformation("Updated family due with Id: {DuesId}", updatedDue.DuesId);
            return _mapper.Map<FamilyDueDto>(updatedDue);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting family due with Id: {Id}", id);
            await _repository.DeleteAsync(id);
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
