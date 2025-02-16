using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchData;
using ChurchData.DTOs;

namespace ChurchContracts
{
    public interface IFamilyMemberService
    {
        // Submission & Approval
        Task<ServiceResponse> SubmitFamilyMemberAsync(PendingFamilyMemberRequestDto requestDto);
        Task<ServiceResponse> ApproveFamilyMemberAsync(FamilyMemberApprovalDto approvalDto);

        // Data retrieval endpoints
        Task<ServiceResponse<FamilyMemberDto>> GetFamilyMemberByIdAsync(int memberId);
        Task<ServiceResponse<IEnumerable<FamilyMemberDto>>> GetFamilyMembersFilteredAsync(FamilyMemberFilterRequest filterRequest);

        // New method for retrieving all family members
        Task<ServiceResponse<IEnumerable<FamilyMemberDto>>> GetAllFamilyMembersAsync(int? parishId, int? familyId);
    }
}
