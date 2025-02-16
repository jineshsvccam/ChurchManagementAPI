using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface IFamilyDueRepository
    {
        Task<IEnumerable<FamilyDue>> GetAllAsync(int? parishId);
        Task<FamilyDue?> GetByIdAsync(int id);
        Task<FamilyDue> AddAsync(FamilyDue familyDue);
        Task<FamilyDue> UpdateAsync(FamilyDue familyDue);
        Task DeleteAsync(int id);
    }
}
