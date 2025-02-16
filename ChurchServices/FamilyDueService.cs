using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
