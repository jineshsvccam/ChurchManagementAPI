using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;

namespace ChurchServices
{
    public class FamilyDueService : IFamilyDueService
    {
        private readonly IFamilyDueRepository _repository;
        private readonly IMapper _mapper;

        public FamilyDueService(IFamilyDueRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FamilyDueDto>> GetAllAsync(int? parishId)
        {
            var dues = await _repository.GetAllAsync(parishId);
            return dues.Select(_mapper.Map<FamilyDue, FamilyDueDto>);
        }

        public async Task<FamilyDueDto?> GetByIdAsync(int id)
        {
            var due = await _repository.GetByIdAsync(id);
            return due == null ? null : _mapper.Map<FamilyDue, FamilyDueDto>(due);
        }

        public async Task<FamilyDueDto> AddAsync(FamilyDueDto dto)
        {
            var due = _mapper.Map<FamilyDue>(dto);
            var addedDue = await _repository.AddAsync(due);
            return _mapper.Map<FamilyDueDto>(addedDue);
        }

        public async Task<FamilyDueDto> UpdateAsync(int id, FamilyDueDto dto)
        {
            var existingDue = await _repository.GetByIdAsync(id);
            if (existingDue == null)
                throw new KeyNotFoundException("Family Due record not found");

            var updatedDue = _mapper.Map(dto, existingDue);
            var result = await _repository.UpdateAsync(updatedDue);
            return _mapper.Map<FamilyDueDto>(result);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
        public async Task<IEnumerable<FamilyDueDto>> AddOrUpdateAsync(IEnumerable<FamilyDueDto> requests)
        {
            var processedEntries = new List<FamilyDue>();

            foreach (var request in requests)
            {
                // Map DTO to entity
                var familyEntity = _mapper.Map<FamilyDue>(request);
                if (request.Action == "INSERT")
                {
                    var createdFamily = await _repository.AddAsync(familyEntity);
                    processedEntries.Add(createdFamily);
                }
                else if (request.Action == "UPDATE")
                {
                    var updatedFamily = await _repository.UpdateAsync(familyEntity);
                    processedEntries.Add(updatedFamily);
                }
                else
                {
                    throw new ArgumentException("Invalid action specified");
                }
            }
            return _mapper.Map<IEnumerable<FamilyDueDto>>(processedEntries);
        }


    }
}
