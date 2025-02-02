using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class FamilyRepository : IFamilyRepository
    {
        private readonly ApplicationDbContext _context;

        public FamilyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Family>> GetFamiliesAsync(int? parishId, int? unitId, int? familyId)
        {
            var query = _context.Families.AsQueryable();

            if (parishId.HasValue)
            {
                query = query.Where(f => f.ParishId == parishId.Value);
            }

            if (unitId.HasValue)
            {
                query = query.Where(f => f.UnitId == unitId.Value);
            }
            if (familyId.HasValue)
            {
                query = query.Where(f => f.FamilyId == familyId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Family?> GetByIdAsync(int id)
        {
            return await _context.Families.FindAsync(id);
        }

        public async Task<Family> AddAsync(Family family)
        {
            await _context.Families.AddAsync(family);
            await _context.SaveChangesAsync();
            return family;
        }

        public async Task<Family> UpdateAsync(Family family)
        {
            var existingFamily = await _context.Families.FindAsync(family.FamilyId);
            if (existingFamily != null)
            {
                _context.Entry(existingFamily).CurrentValues.SetValues(family);
                await _context.SaveChangesAsync();
                return family;
            }
            else
            {
                throw new KeyNotFoundException("Family not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var family = await _context.Families.FindAsync(id);
            if (family != null)
            {
                _context.Families.Remove(family);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Family not found");
            }
        }
    }

}
