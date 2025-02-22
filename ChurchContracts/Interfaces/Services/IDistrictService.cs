using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IDistrictService
    {
        Task<IEnumerable<DistrictDto>> GetAllAsync();
        Task<DistrictDto> GetByIdAsync(int id);
        Task<DistrictDto> AddAsync(DistrictDto districtDto);
        Task<DistrictDto> UpdateAsync(DistrictDto districtDto);
        Task DeleteAsync(int id);
    }
}
