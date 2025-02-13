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
    public class UnitRepository : IUnitRepository
    {
        private readonly ApplicationDbContext _context;

        public UnitRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Unit>> GetAllAsync(int? parishId)
        {
            var query = _context.Units.AsQueryable();
            if (parishId.HasValue)
                query = query.Where(u => u.ParishId == parishId.Value);
            var result = await query.ToListAsync();
            return result;
        }

        public async Task<Unit?> GetByIdAsync(int id)
        {
            return await _context.Units.FindAsync(id);
        }

        public async Task<Unit> AddAsync(Unit unit)
        {
            await _context.Units.AddAsync(unit);
            await _context.SaveChangesAsync();
            return unit;
        }

        public async Task<Unit> UpdateAsync(Unit unit)
        {
            var existingUnit = await _context.Units.FindAsync(unit.UnitId);
            if (existingUnit != null)
            {
                _context.Entry(existingUnit).CurrentValues.SetValues(unit);
                await _context.SaveChangesAsync();
                return unit;
            }
            else
            {
                throw new KeyNotFoundException("Unit not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit != null)
            {
                _context.Units.Remove(unit);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Unit not found");
            }
        }
    }

}
