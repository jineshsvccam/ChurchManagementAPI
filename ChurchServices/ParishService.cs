using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;

namespace ChurchServices
{
    public class ParishService : IParishService
    {
        private readonly IParishRepository _parishRepository;

        public ParishService(IParishRepository parishRepository)
        {
            _parishRepository = parishRepository;
        }

        public async Task<IEnumerable<Parish>> GetAllAsync()
        {
            return await _parishRepository.GetAllAsync();
        }

        public async Task<Parish?> GetByIdAsync(int id)
        {
            return await _parishRepository.GetByIdAsync(id);
        }

        public async Task<Parish> AddAsync(Parish parish)
        {
            return await _parishRepository.AddAsync(parish);
        }

        public async Task UpdateAsync(Parish parish)
        {
            await _parishRepository.UpdateAsync(parish);
        }

        public async Task DeleteAsync(int id)
        {
            await _parishRepository.DeleteAsync(id);
        }
        public async Task<ParishDetailsDto> GetParishDetailsAsync(int parishId, bool includeTransactions = false, bool includeFamilyMembers = false)
        {
            return await _parishRepository.GetParishDetailsAsync(parishId, includeTransactions, includeFamilyMembers);
        }
    }

}
