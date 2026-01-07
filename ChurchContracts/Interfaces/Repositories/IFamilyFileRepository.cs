using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchData.Entities;

namespace ChurchContracts
{
    public interface IFamilyFileRepository
    {
        Task<FamilyFile?> GetByIdAsync(Guid fileId);

        Task<IEnumerable<FamilyFile>> GetByFamilyAsync(int familyId);

        Task<IEnumerable<FamilyFile>> GetByMemberAsync(int familyId, int memberId);

        Task<IEnumerable<FamilyFile>> GetByTypeAsync(
            int familyId,
            int? memberId,
            string fileType
        );

        Task AddAsync(FamilyFile familyFile);

        Task UpdateAsync(FamilyFile familyFile);

        Task DeleteAsync(Guid fileId);
    }
}
