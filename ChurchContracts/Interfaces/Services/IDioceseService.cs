using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IDioceseService
    {
        Task<IEnumerable<DioceseDto>> GetAllAsync();
        Task<DioceseDto> GetByIdAsync(int id);
        Task AddAsync(DioceseDto dioceseDto);
        Task UpdateAsync(DioceseDto dioceseDto);
        Task DeleteAsync(int id);
    }
}
