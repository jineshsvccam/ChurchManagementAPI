using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IDioceseService
    {
        Task<IEnumerable<DioceseDto>> GetAllAsync();
        Task<DioceseDto> GetByIdAsync(int id);
        Task<DioceseDto> AddAsync(DioceseDto dioceseDto);
        Task<DioceseDto> UpdateAsync(DioceseDto dioceseDto);
        Task DeleteAsync(int id);
    }
}
