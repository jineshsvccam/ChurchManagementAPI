using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchData.DTOs;

namespace ChurchContracts
{
    public interface IFamilyDueService
    {
        Task<IEnumerable<FamilyDueDto>> GetAllAsync(int? parishId);
        Task<FamilyDueDto?> GetByIdAsync(int id);
        Task<FamilyDueDto> AddAsync(FamilyDueDto dto);
        Task<FamilyDueDto> UpdateAsync(int id, FamilyDueDto dto);
        Task DeleteAsync(int id);
    }
}
