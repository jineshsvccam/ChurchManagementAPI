using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface IParishRepository
    {
        Task<IEnumerable<Parish>> GetAllAsync();
        Task<Parish?> GetByIdAsync(int id);
        Task<Parish> AddAsync(Parish parish);
        Task UpdateAsync(Parish parish);
        Task DeleteAsync(int id);
    }

}
