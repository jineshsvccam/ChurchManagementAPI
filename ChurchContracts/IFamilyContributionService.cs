using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData.DTOs;

namespace ChurchContracts
{
    public interface IFamilyContributionService
    {
        Task<IEnumerable<FamilyContributionDto>> GetAllAsync(int? parishId);
        Task<FamilyContributionDto?> GetByIdAsync(int id);
        Task<FamilyContributionDto> AddAsync(FamilyContributionDto familyContributionDto);
        Task<FamilyContributionDto> UpdateAsync(FamilyContributionDto familyContributionDto);
        Task DeleteAsync(int id);
    }
}
