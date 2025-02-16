using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData;

namespace ChurchContracts
{
    public interface IFamilyContributionRepository
    {
        Task<IEnumerable<FamilyContribution>> GetAllAsync(int? parishId);
        Task<FamilyContribution?> GetByIdAsync(int id);
        Task<FamilyContribution> AddAsync(FamilyContribution familyContribution);
        Task<FamilyContribution> UpdateAsync(FamilyContribution familyContribution);
        Task DeleteAsync(int id);
    }
}
