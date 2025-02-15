using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;
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

        public async Task<FamilyMember> GetFamilyMemberByIdAsync(int memberId)
        {
            return await _context.FamilyMembers
                .Include(fm => fm.Contacts)
                .Include(fm => fm.Identity)
                .Include(fm => fm.Occupation)
                .Include(fm => fm.Sacraments)               
                .Include(fm => fm.Files)
                .Include(fm => fm.Lifecycle)
                 .Include(fm => fm.Relations)
                .FirstOrDefaultAsync(fm => fm.MemberId == memberId);

            
        }

        public async Task<IEnumerable<FamilyMember>> GetFamilyMembersFilteredAsync(FamilyMemberFilterRequest filterRequest)
        {
            IQueryable<FamilyMember> query = _context.FamilyMembers
                .Include(fm => fm.Contacts)
                .Include(fm => fm.Identity)
                .Include(fm => fm.Occupation)
                .Include(fm => fm.Sacraments)
                .Include(fm => fm.Relations)
                .Include(fm => fm.Files)
                .Include(fm => fm.Lifecycle);

            // Filter by ActiveMember
            if (filterRequest.Filters.TryGetValue("ActiveMember", out string activeMemberVal))
            {
                if (bool.TryParse(activeMemberVal, out bool isActive))
                {
                    query = query.Where(fm => fm.ActiveMember == isActive);
                }
            }

            // Filter by FirstName
            if (filterRequest.Filters.TryGetValue("FirstName", out string firstName))
            {
                query = query.Where(fm => fm.FirstName.Contains(firstName));
            }

            // You can add additional filters here as needed.

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<FamilyMember>> GetAllFamilyMembersAsync(int? parishId, int? familyId)
        {
            IQueryable<FamilyMember> query = _context.FamilyMembers
                 .Include(fm => fm.Contacts)
                 .Include(fm => fm.Identity)
                 .Include(fm => fm.Occupation)
                 .Include(fm => fm.Sacraments)
                 .Include(fm => fm.Relations)
                 .Include(fm => fm.Files)
                 .Include(fm => fm.Lifecycle);

            if (parishId.HasValue)
            {
                query = query.Where(fm => fm.ParishId == parishId.Value);
            }

            if (familyId.HasValue)
            {
                query = query.Where(fm => fm.FamilyId == familyId.Value);
            }

            return await query.ToListAsync();
        }

    }
}
