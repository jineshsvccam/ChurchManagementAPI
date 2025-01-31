using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;

namespace ChurchServices
{
    public class DioceseService : IDioceseService
    {
        private readonly IDioceseRepository _dioceseRepository;

        public DioceseService(IDioceseRepository dioceseRepository)
        {
            _dioceseRepository = dioceseRepository;
        }

        public async Task<IEnumerable<Diocese>> GetAllAsync()
        {
            return await _dioceseRepository.GetAllAsync();
        }

        public async Task<Diocese> GetByIdAsync(int id)
        {
            return await _dioceseRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Diocese diocese)
        {
            await _dioceseRepository.AddAsync(diocese);
        }

        public async Task UpdateAsync(Diocese diocese)
        {
            await _dioceseRepository.UpdateAsync(diocese);
        }

        public async Task DeleteAsync(int id)
        {
            await _dioceseRepository.DeleteAsync(id);
        }
    }

}
