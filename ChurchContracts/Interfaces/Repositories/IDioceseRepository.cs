using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface IDioceseRepository
    {
        Task<IEnumerable<Diocese>> GetAllAsync();
        Task<Diocese> GetByIdAsync(int id);
        Task AddAsync(Diocese diocese);
        Task UpdateAsync(Diocese diocese);
        Task DeleteAsync(int id);
    }
}
