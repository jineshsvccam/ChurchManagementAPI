using ChurchContracts;
using ChurchData;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchRepositories
{
    public class FamilyContributionRepository : IFamilyContributionRepository
    {
        private readonly ApplicationDbContext _context;

        public FamilyContributionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FamilyContribution>> GetAllAsync(int? parishId)
        {
            var query = _context.FamilyContributions.AsQueryable();
            if (parishId.HasValue)
                query = query.Where(fc => fc.ParishId == parishId.Value);
            return await query.ToListAsync();
        }

        public async Task<FamilyContribution?> GetByIdAsync(int id)
        {
            return await _context.FamilyContributions.FindAsync(id);
        }

        public async Task<FamilyContribution> AddAsync(FamilyContribution familyContribution)
        {
            familyContribution.TransactionDate = DateTime.SpecifyKind(familyContribution.TransactionDate, DateTimeKind.Utc);
            await _context.FamilyContributions.AddAsync(familyContribution);
            await _context.SaveChangesAsync();
            return familyContribution;
        }

        public async Task<FamilyContribution> UpdateAsync(FamilyContribution familyContribution)
        {
            var existingContribution = await _context.FamilyContributions.FindAsync(familyContribution.ContributionId);
            if (existingContribution != null)
            {
                _context.Entry(existingContribution).CurrentValues.SetValues(familyContribution);
                familyContribution.TransactionDate = DateTime.SpecifyKind(familyContribution.TransactionDate, DateTimeKind.Utc);
                await _context.SaveChangesAsync();
                return familyContribution;
            }
            else
            {
                throw new KeyNotFoundException("Family Contribution not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var contribution = await _context.FamilyContributions.FindAsync(id);
            if (contribution != null)
            {
                _context.FamilyContributions.Remove(contribution);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Family Contribution not found");
            }
        }
    }
}
