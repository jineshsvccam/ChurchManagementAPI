using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;


namespace ChurchServices
{
    public class FamilyService : IFamilyService
    {
        private readonly IFamilyRepository _familyRepository;

        public FamilyService(IFamilyRepository familyRepository)
        {
            _familyRepository = familyRepository;
        }

        public async Task<IEnumerable<Family>> GetFamiliesAsync(int? parishId, int? unitId, int? familyId)
        {
            return await _familyRepository.GetFamiliesAsync(parishId, unitId, familyId);
        }

        public async Task<Family?> GetByIdAsync(int id)
        {
            return await _familyRepository.GetByIdAsync(id);
        }


        public async Task<IEnumerable<Family>> AddOrUpdateAsync(IEnumerable<Family> requests)
        {
            var createdFamilies = new List<Family>();

            foreach (var request in requests)
            {
                if (request.Action == "INSERT")
                {
                    var createdFamily = await AddAsync(request);
                    createdFamilies.Add(createdFamily);
                }
                else if (request.Action == "UPDATE")
                {
                    var createdFamily = await UpdateAsync(request);
                    createdFamilies.Add(createdFamily);
                }
                else
                {
                    throw new ArgumentException("Invalid action specified");
                }
            }
            return createdFamilies;
        }
        public async Task<Family> AddAsync(Family family)
        {
            var addedFamily = await _familyRepository.AddAsync(family);
            return addedFamily;
        }
        public async Task<Family> UpdateAsync(Family family)
        {
            if (family.JoinDate.HasValue)
            {
                family.JoinDate = DateTime.SpecifyKind(family.JoinDate.Value, DateTimeKind.Utc);
            }
            var updatedFamily = await _familyRepository.UpdateAsync(family);
            return updatedFamily;

        }
        public async Task DeleteAsync(int id)
        {
            await _familyRepository.DeleteAsync(id);
        }
    }

}
