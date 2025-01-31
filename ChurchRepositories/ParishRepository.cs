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
    public class ParishRepository : IParishRepository
    {
        private readonly ApplicationDbContext _context;

        public ParishRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Parish>> GetAllAsync()
        {
            return await _context.Parishes.ToListAsync();
        }

        public async Task<Parish?> GetByIdAsync(int id)
        {
            return await _context.Parishes.FindAsync(id);
        }

        public async Task<Parish> AddAsync(Parish parish)
        {
            await _context.Parishes.AddAsync(parish);
            await _context.SaveChangesAsync();
            return parish;
        }

        public async Task UpdateAsync(Parish parish)
        {
            var existingParish = await _context.Parishes.FindAsync(parish.ParishId);
            if (existingParish != null)
            {
                _context.Entry(existingParish).CurrentValues.SetValues(parish);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Parish not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var parish = await _context.Parishes.FindAsync(id);
            if (parish != null)
            {
                _context.Parishes.Remove(parish);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Parish not found");
            }
        }
    }

}
