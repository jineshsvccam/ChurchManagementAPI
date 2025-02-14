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
        public async Task<PendingFamilyMemberAction> AddPendingActionAsync(PendingFamilyMemberAction action)
        {
            _context.PendingFamilyMemberActions.Add(action);
            await _context.SaveChangesAsync();
            return action;
        }

        public async Task<PendingFamilyMemberAction> GetPendingActionByIdAsync(int actionId)
        {
            return await _context.PendingFamilyMemberActions
                .FirstOrDefaultAsync(a => a.ActionId == actionId && a.ApprovalStatus == "Pending");
        }

        public async Task UpdatePendingActionAsync(PendingFamilyMemberAction action)
        {
            _context.PendingFamilyMemberActions.Update(action);
            await _context.SaveChangesAsync();
        }
        public async Task<FamilyMember> InsertApprovedFamilyMemberAsync(FamilyMember familyMember)
        {
            _context.FamilyMembers.Add(familyMember);
            await _context.SaveChangesAsync();
            return familyMember;
        }
    }
}
