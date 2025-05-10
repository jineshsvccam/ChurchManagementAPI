using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IFamilyMemberService
    {
        // Submission & Approval
        Task<ServiceResponse> SubmitFamilyMemberAsync(PendingFamilyMemberRequestDto requestDto);
        Task<ServiceResponse> ApproveFamilyMemberAsync(FamilyMemberApprovalDto approvalDto);

        // Data retrieval endpoints
        Task<ServiceResponse<FamilyMemberDto>> GetFamilyMemberByIdAsync(int memberId);
        Task<ServiceResponse<IEnumerable<FamilyMemberDto>>> GetFamilyMembersFilteredAsync(int parishId, int? familyId,  FamilyMemberFilterRequest filterRequest);

        // New method for retrieving all family members
        Task<ServiceResponse<IEnumerable<FamilyMemberDto>>> GetAllFamilyMembersAsync(int? parishId, int? familyId);
    }
}
