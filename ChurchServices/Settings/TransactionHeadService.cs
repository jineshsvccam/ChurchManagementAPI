using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Settings
{
    public class TransactionHeadService : ITransactionHeadService
    {
        private readonly ITransactionHeadRepository _transactionHeadRepository;
        private readonly ILogger<TransactionHeadService> _logger;
        private readonly IMapper _mapper;

        public TransactionHeadService(ITransactionHeadRepository transactionHeadRepository,
                                      ILogger<TransactionHeadService> logger,
                                      IMapper mapper)
        {
            _transactionHeadRepository = transactionHeadRepository ?? throw new ArgumentNullException(nameof(transactionHeadRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<TransactionHeadDto>> GetTransactionHeadsAsync(int? parishId, int? headId)
        {
            _logger.LogInformation("Fetching transaction heads for ParishId: {ParishId}, HeadId: {HeadId}", parishId, headId);
            var result = await _transactionHeadRepository.GetTransactionHeadsAsync(parishId, headId);
            var resultDto = _mapper.Map<IEnumerable<TransactionHeadDto>>(result);
            _logger.LogInformation("Fetched {Count} transaction heads successfully.", resultDto?.Count() ?? 0);
            return resultDto;
        }

        public async Task<TransactionHeadDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching transaction head by Id: {Id}", id);
            var result = await _transactionHeadRepository.GetByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Transaction head not found with Id: {Id}", id);
                return null;
            }
            return _mapper.Map<TransactionHeadDto>(result);
        }

        public async Task<IEnumerable<TransactionHeadDto>> AddOrUpdateAsync(IEnumerable<TransactionHeadDto> requests)
        {
            _logger.LogInformation("Processing {Count} transaction head(s) for AddOrUpdate.", requests.Count());
            var processedEntities = new List<TransactionHead>();

            foreach (var requestDto in requests)
            {
                // Map DTO to entity
                var transactionHeadEntity = _mapper.Map<TransactionHead>(requestDto);

                if (requestDto.Action?.ToUpper() == "INSERT")
                {
                    _logger.LogInformation("Adding new transaction head: {HeadName}", transactionHeadEntity.HeadName);
                    var createdEntity = await _transactionHeadRepository.AddAsync(transactionHeadEntity);
                    processedEntities.Add(createdEntity);
                }
                else if (requestDto.Action?.ToUpper() == "UPDATE")
                {
                    _logger.LogInformation("Updating transaction head Id: {Id}", transactionHeadEntity.HeadId);
                    var updatedEntity = await _transactionHeadRepository.UpdateAsync(transactionHeadEntity);
                    processedEntities.Add(updatedEntity);
                }
                else
                {
                    _logger.LogWarning("Invalid action '{Action}' specified for transaction head Id: {Id}", requestDto.Action, transactionHeadEntity.HeadId);
                    throw new ArgumentException("Invalid action specified");
                }
            }

            _logger.LogInformation("Successfully processed {Count} transaction heads for AddOrUpdate.", processedEntities.Count);
            return _mapper.Map<IEnumerable<TransactionHeadDto>>(processedEntities);
        }

        public async Task<TransactionHeadDto> AddAsync(TransactionHeadDto transactionHeadDto)
        {
            _logger.LogInformation("Adding transaction head: {HeadName}", transactionHeadDto.HeadName);
            var entity = _mapper.Map<TransactionHead>(transactionHeadDto);
            var addedEntity = await _transactionHeadRepository.AddAsync(entity);
            _logger.LogInformation("Successfully added transaction head Id: {Id}", addedEntity.HeadId);
            return _mapper.Map<TransactionHeadDto>(addedEntity);
        }

        public async Task<TransactionHeadDto> UpdateAsync(TransactionHeadDto transactionHeadDto)
        {
            _logger.LogInformation("Updating transaction head Id: {Id}", transactionHeadDto.HeadId);
            var entity = _mapper.Map<TransactionHead>(transactionHeadDto);
            var updatedEntity = await _transactionHeadRepository.UpdateAsync(entity);
            _logger.LogInformation("Successfully updated transaction head Id: {Id}", updatedEntity.HeadId);
            return _mapper.Map<TransactionHeadDto>(updatedEntity);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting transaction head Id: {Id}", id);
            await _transactionHeadRepository.DeleteAsync(id);
            _logger.LogInformation("Successfully deleted transaction head Id: {Id}", id);
        }
    }
}
