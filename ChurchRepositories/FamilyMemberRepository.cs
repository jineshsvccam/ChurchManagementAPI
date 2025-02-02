using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class FamilyMemberRepository : IFamilyMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public FamilyMemberRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FamilyMember>> GetFamilyMembersAsync(int? parishId, int? familyId, int? memberId)
        {
            var query = _context.FamilyMembers.AsQueryable();

            if (parishId.HasValue)
            {
                query = query.Where(fm => fm.Family.ParishId == parishId.Value);
            }

            if (familyId.HasValue)
            {
                query = query.Where(fm => fm.FamilyId == familyId.Value);
            }

            if (memberId.HasValue)
            {
                query = query.Where(fm => fm.MemberId == memberId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<FamilyMember?> GetByIdAsync(int id)
        {
            return await _context.FamilyMembers.FindAsync(id);
        }

        public async Task<FamilyMember> AddAsync(FamilyMember familyMember)
        {
            await _context.FamilyMembers.AddAsync(familyMember);
            await _context.SaveChangesAsync();
            return familyMember;
        }

        public async Task<FamilyMember> UpdateAsync(FamilyMember familyMember)
        {
            var existingFamilyMember = await _context.FamilyMembers.FindAsync(familyMember.MemberId);
            if (existingFamilyMember != null)
            {
                _context.Entry(existingFamilyMember).CurrentValues.SetValues(familyMember);
                await _context.SaveChangesAsync();
                return familyMember;
            }
            else
            {
                throw new KeyNotFoundException("FamilyMember not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var familyMember = await _context.FamilyMembers.FindAsync(id);
            if (familyMember != null)
            {
                _context.FamilyMembers.Remove(familyMember);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("FamilyMember not found");
            }
        }
    }
}
