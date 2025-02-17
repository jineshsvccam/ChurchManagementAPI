using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IUnitService
    {
        Task<IEnumerable<UnitDto>> GetAllAsync(int? parishId);
        Task<UnitDto?> GetByIdAsync(int id);
        Task<UnitDto> AddAsync(UnitDto unitDto);
        Task<UnitDto> UpdateAsync(UnitDto unitDto);
        Task DeleteAsync(int id);
        Task<IEnumerable<UnitDto>> AddOrUpdateAsync(IEnumerable<UnitDto> requests);
    }
}
