using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;

namespace ChurchServices
{
    public class DistrictService : IDistrictService
    {
        private readonly IDistrictRepository _districtRepository;

        public DistrictService(IDistrictRepository districtRepository)
        {
            _districtRepository = districtRepository;
        }

        public async Task<IEnumerable<District>> GetAllAsync()
        {
            return await _districtRepository.GetAllAsync();
        }

        public async Task<District> GetByIdAsync(int id)
        {
            return await _districtRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(District district)
        {
            await _districtRepository.AddAsync(district);
        }

        public async Task UpdateAsync(District district)
        {
            await _districtRepository.UpdateAsync(district);
        }

        public async Task DeleteAsync(int id)
        {
            await _districtRepository.DeleteAsync(id);
        }
    }

}
