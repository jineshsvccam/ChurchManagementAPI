using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface IDistrictRepository
    {
        Task<IEnumerable<District>> GetAllAsync();
        Task<District> GetByIdAsync(int id);
        Task AddAsync(District district);
        Task UpdateAsync(District district);
        Task DeleteAsync(int id);
    }
}
