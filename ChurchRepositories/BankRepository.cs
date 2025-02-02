using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchContracts.ChurchContracts;
using ChurchData;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class BankRepository : IBankRepository
    {
        private readonly ApplicationDbContext _context;

        public BankRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Bank>> GetBanksAsync(int? parishId, int? bankId)
        {
            var query = _context.Banks.AsQueryable();

            if (parishId.HasValue)
            {
                query = query.Where(b => b.ParishId == parishId.Value);
            }

            if (bankId.HasValue)
            {
                query = query.Where(b => b.BankId == bankId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Bank?> GetByIdAsync(int id)
        {
            return await _context.Banks.FindAsync(id);
        }

        public async Task<Bank> AddAsync(Bank bank)
        {
            await _context.Banks.AddAsync(bank);
            await _context.SaveChangesAsync();
            return bank;
        }

        public async Task<Bank> UpdateAsync(Bank bank)
        {
            var existingBank = await _context.Banks.FindAsync(bank.BankId);
            if (existingBank != null)
            {
                _context.Entry(existingBank).CurrentValues.SetValues(bank);
                await _context.SaveChangesAsync();
                return bank;
            }
            else
            {
                throw new KeyNotFoundException("Bank not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var bank = await _context.Banks.FindAsync(id);
            if (bank != null)
            {
                _context.Banks.Remove(bank);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Bank not found");
            }
        }
    }
}
