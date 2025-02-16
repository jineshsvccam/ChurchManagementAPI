using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface IFamilyService
    {
        Task<IEnumerable<Family>> GetFamiliesAsync(int? parishId, int? unitId, int? familyId);
        Task<Family?> GetByIdAsync(int id);
        Task<IEnumerable<Family>> AddOrUpdateAsync(IEnumerable<Family> requests);
        Task<Family> UpdateAsync(Family family);
        Task DeleteAsync(int id);
        Task<Family> AddAsync(Family family);
    }

}
