using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IDistrictService
    {
        Task<IEnumerable<DistrictDto>> GetAllAsync();
        Task<DistrictDto> GetByIdAsync(int id);
        Task AddAsync(DistrictDto districtDto);
        Task UpdateAsync(DistrictDto districtDto);
        Task DeleteAsync(int id);
    }
}
