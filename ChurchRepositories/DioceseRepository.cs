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
    public class DioceseRepository : IDioceseRepository
    {
        private readonly ApplicationDbContext _context;

        public DioceseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Diocese>> GetAllAsync()
        {
            return await _context.Dioceses.ToListAsync();
        }

        public async Task<Diocese> GetByIdAsync(int id)
        {
            return await _context.Dioceses
            .Include(d => d.Districts) // Include related Districts
            .FirstOrDefaultAsync(d => d.DioceseId == id);
        }

        public async Task AddAsync(Diocese diocese)
        {
            await _context.Dioceses.AddAsync(diocese);
            await _context.SaveChangesAsync();            
        }

        public async Task UpdateAsync(Diocese diocese)
        {
            var existingDiocese = await _context.Dioceses.FindAsync(diocese.DioceseId);
            if (existingDiocese != null)
            {
                _context.Entry(existingDiocese).CurrentValues.SetValues(diocese);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Diocese not found");
            }
        }


        public async Task DeleteAsync(int id)
        {
            var diocese = await _context.Dioceses.FindAsync(id);
            if (diocese != null)
            {
                _context.Dioceses.Remove(diocese);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Diocese not found");
            }
        }

    }

}
