using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface IUnitRepository
    {
        Task<IEnumerable<Unit>> GetAllAsync();
        Task<Unit?> GetByIdAsync(int id);
        Task<Unit> AddAsync(Unit unit);
        Task UpdateAsync(Unit unit);
        Task DeleteAsync(int id);
    }
}
