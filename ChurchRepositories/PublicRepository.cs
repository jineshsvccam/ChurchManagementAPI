using ChurchContracts.Interfaces.Repositories;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class PublicRepository : IPublicRepository
    {
        private readonly ApplicationDbContext _context;

        public PublicRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ParishesAllDto> GetAllParishesAsync()
        {
            var dioceses = await _context.Dioceses
                .Include(d => d.Districts)
                    .ThenInclude(ds => ds.Parishes)
                        .ThenInclude(p => p.Units)
                            .ThenInclude(u => u.Families)
                .ToListAsync();

            var result = new ParishesAllDto
            {
                AllParishes = dioceses.Select(d => new DioceseDetailDto
                {
                    DioceseId = d.DioceseId,
                    DioceseName = d.DioceseName,
                    Districts = d.Districts.Select(ds => new DistrictSimpleDto
                    {
                        DistrictId = ds.DistrictId,
                        DistrictName = ds.DistrictName,
                        DioceseId = ds.DioceseId,
                        Parishes = ds.Parishes.Select(p => new ParishSimpleDto
                        {
                            ParishId = p.ParishId,
                            ParishName = p.ParishName,
                            ParishLocation = p.ParishLocation,
                            DistrictId = p.DistrictId,
                            Units = p.Units.Select(u => new UnitSimpleDto
                            {
                                UnitId = u.UnitId,
                                ParishId = u.ParishId,
                                UnitName = u.UnitName,
                                Families = u.Families.Select(f => new FamilySimpleDto
                                {
                                    FamilyId = f.FamilyId,
                                    FamilyName = f.FamilyName,
                                    FamilyNumber = f.FamilyNumber,
                                    Status = f.Status,
                                    HeadName = f.HeadName,
                                    ParishId = f.ParishId
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return result;
        }

    }
}
