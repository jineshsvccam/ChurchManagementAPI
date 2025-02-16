using ChurchData;
using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IParishService
    {
        Task<IEnumerable<Parish>> GetAllAsync();
        Task<Parish?> GetByIdAsync(int id);
        Task<Parish> AddAsync(Parish parish);
        Task UpdateAsync(Parish parish);
        Task DeleteAsync(int id);
        Task<ParishDetailsDto> GetParishDetailsAsync(int parishId, bool includeFamilyMembers = false);
    }

}
