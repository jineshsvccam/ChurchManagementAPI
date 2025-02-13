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

        public async Task<IEnumerable<Unit>> GetAllAsync(int? parishId)
        {
            return await _unitRepository.GetAllAsync(parishId);
        }

        public async Task<Unit?> GetByIdAsync(int id)
        {
            return await _unitRepository.GetByIdAsync(id);
        }

        public async Task<Unit> AddAsync(Unit unit)
        {
            return await _unitRepository.AddAsync(unit);
        }

        public async Task<Unit> UpdateAsync(Unit unit)
        {
            await _unitRepository.UpdateAsync(unit);
            return unit;
        }

        public async Task DeleteAsync(int id)
        {
            await _unitRepository.DeleteAsync(id);
        }
        public async Task<IEnumerable<Unit>> AddOrUpdateAsync(IEnumerable<Unit> requests)
        {
            var createdUnits = new List<Unit>();
            foreach (var request in requests)
            {
                if (request.Action == "INSERT")
                {
                    var createdUnit = await _unitRepository.AddAsync(request);
                    createdUnits.Add(createdUnit);
                }
                else if (request.Action == "UPDATE")
                {
                    var createdUnit = await _unitRepository.UpdateAsync(request);
                    createdUnits.Add(createdUnit);
                }
                else
                {
                    throw new ArgumentException("Invalid action specified");
                }
            }
            return createdUnits;
        }
    }
}
