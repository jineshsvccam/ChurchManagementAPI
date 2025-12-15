using System.Collections.Generic;
using System.Threading.Tasks;

using ChurchData;
using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IFamilyMemberRepository
    {
        // Pending actions
        Task<PendingFamilyMemberAction> AddPendingActionAsync(PendingFamilyMemberAction action);
        Task<PendingFamilyMemberAction> GetPendingActionByIdAsync(int actionId);
        Task UpdatePendingActionAsync(PendingFamilyMemberAction action);
        Task<IEnumerable<PendingFamilyMemberAction>> GetPendingApprovalListAsync(int parishId);

        // Approved family member insertion
        Task<FamilyMember> InsertApprovedFamilyMemberAsync(FamilyMember familyMember);

        // Data retrieval for approved family members
        Task<FamilyMember> GetFamilyMemberByIdAsync(int memberId);
        Task<IEnumerable<FamilyMember>> GetFamilyMembersFilteredAsync(int parishId, int? familyId, FamilyMemberFilterRequest filterRequest);

        Task<IEnumerable<FamilyMember>> GetAllFamilyMembersAsync(int? parishId, int? familyId);
    }
}
