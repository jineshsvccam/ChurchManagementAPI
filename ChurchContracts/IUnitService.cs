using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface IUnitService
    {
        Task<IEnumerable<Unit>> GetAllAsync(int? parishId);
        Task<Unit?> GetByIdAsync(int id);
        Task<Unit> AddAsync(Unit unit);
        Task<Unit> UpdateAsync(Unit unit);
        Task DeleteAsync(int id);
        Task<IEnumerable<Unit>> AddOrUpdateAsync(IEnumerable<Unit> units);
    }

}
