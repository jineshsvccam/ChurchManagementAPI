using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchRepositories.Queries.Models;

namespace ChurchRepositories.Queries
{
    public interface IFamilyQueryRepository
    {
        Task<IEnumerable<FamilyWithPhotoInfo>> GetFamiliesWithPhotoAsync(
            int? parishId,
            int? unitId,
            int? familyId);
    }

}
