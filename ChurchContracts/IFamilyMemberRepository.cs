using ChurchData;

namespace ChurchContracts
{
    public interface IFamilyMemberRepository
    {

        Task<PendingFamilyMemberAction> AddPendingActionAsync(PendingFamilyMemberAction action);
        Task<PendingFamilyMemberAction> GetPendingActionByIdAsync(int actionId);
        Task UpdatePendingActionAsync(PendingFamilyMemberAction action);
        Task<FamilyMember> InsertApprovedFamilyMemberAsync(FamilyMember familyMember);

    }
}
