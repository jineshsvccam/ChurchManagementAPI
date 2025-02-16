using ChurchContracts;
using ChurchData;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchRepositories
{
    public class FamilyDueRepository : IFamilyDueRepository
    {
        private readonly ApplicationDbContext _context;

        public FamilyDueRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FamilyDue>> GetAllAsync(int? parishId)
        {
            var query = _context.FamilyDues.AsQueryable();
            if (parishId.HasValue)
                query = query.Where(fd => fd.ParishId == parishId.Value);
            return await query.ToListAsync();
        }

        public async Task<FamilyDue?> GetByIdAsync(int id)
        {
            return await _context.FamilyDues.FindAsync(id);
        }

        public async Task<FamilyDue> AddAsync(FamilyDue familyDue)
        {
            await _context.FamilyDues.AddAsync(familyDue);
            await _context.SaveChangesAsync();
            return familyDue;
        }

        public async Task<FamilyDue> UpdateAsync(FamilyDue familyDue)
        {
            var existingDue = await _context.FamilyDues.FindAsync(familyDue.DuesId);
            if (existingDue != null)
            {
                _context.Entry(existingDue).CurrentValues.SetValues(familyDue);
                await _context.SaveChangesAsync();
                return familyDue;
            }
            else
            {
                throw new KeyNotFoundException("Family Due record not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var due = await _context.FamilyDues.FindAsync(id);
            if (due != null)
            {
                _context.FamilyDues.Remove(due);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Family Due record not found");
            }
        }
    }
}
