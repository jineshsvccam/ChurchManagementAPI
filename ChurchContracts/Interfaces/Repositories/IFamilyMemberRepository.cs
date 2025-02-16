using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchData;
using ChurchData.DTOs;

namespace ChurchContracts
{
    public interface IFamilyMemberRepository
    {
        // Pending actions
        Task<PendingFamilyMemberAction> AddPendingActionAsync(PendingFamilyMemberAction action);
        Task<PendingFamilyMemberAction> GetPendingActionByIdAsync(int actionId);
        Task UpdatePendingActionAsync(PendingFamilyMemberAction action);

        // Approved family member insertion
        Task<FamilyMember> InsertApprovedFamilyMemberAsync(FamilyMember familyMember);

        // Data retrieval for approved family members
        Task<FamilyMember> GetFamilyMemberByIdAsync(int memberId);
        Task<IEnumerable<FamilyMember>> GetFamilyMembersFilteredAsync(FamilyMemberFilterRequest filterRequest);

        Task<IEnumerable<FamilyMember>> GetAllFamilyMembersAsync(int? parishId, int? familyId);
    }
}
