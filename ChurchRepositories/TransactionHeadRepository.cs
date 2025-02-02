using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class TransactionHeadRepository : ITransactionHeadRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionHeadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TransactionHead>> GetTransactionHeadsAsync(int? parishId, int? headId)
        {
            var query = _context.TransactionHeads.AsQueryable();

            if (parishId.HasValue)
            {
                query = query.Where(th => th.ParishId == parishId.Value);
            }

            if (headId.HasValue)
            {
                query = query.Where(th => th.HeadId == headId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<TransactionHead?> GetByIdAsync(int id)
        {
            return await _context.TransactionHeads.FindAsync(id);
        }

        public async Task<TransactionHead> AddAsync(TransactionHead transactionHead)
        {
            await _context.TransactionHeads.AddAsync(transactionHead);
            await _context.SaveChangesAsync();
            return transactionHead;
        }

        public async Task<TransactionHead> UpdateAsync(TransactionHead transactionHead)
        {
            var existingTransactionHead = await _context.TransactionHeads.FindAsync(transactionHead.HeadId);
            if (existingTransactionHead != null)
            {
                _context.Entry(existingTransactionHead).CurrentValues.SetValues(transactionHead);
                await _context.SaveChangesAsync();
                return transactionHead;
            }
            else
            {
                throw new KeyNotFoundException("TransactionHead not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var transactionHead = await _context.TransactionHeads.FindAsync(id);
            if (transactionHead != null)
            {
                _context.TransactionHeads.Remove(transactionHead);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("TransactionHead not found");
            }
        }
    }
}
