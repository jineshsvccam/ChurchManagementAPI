using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;

namespace ChurchServices
{
    public class UnitService : IUnitService
    {
        private readonly IUnitRepository _unitRepository;

        public UnitService(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        public async Task<IEnumerable<Unit>> GetAllAsync()
        {
            return await _unitRepository.GetAllAsync();
        }

        public async Task<Unit?> GetByIdAsync(int id)
        {
            return await _unitRepository.GetByIdAsync(id);
        }

        public async Task<Unit> AddAsync(Unit unit)
        {
            return await _unitRepository.AddAsync(unit);
        }

        public async Task UpdateAsync(Unit unit)
        {
            await _unitRepository.UpdateAsync(unit);
        }

        public async Task DeleteAsync(int id)
        {
            await _unitRepository.DeleteAsync(id);
        }
    }

}
