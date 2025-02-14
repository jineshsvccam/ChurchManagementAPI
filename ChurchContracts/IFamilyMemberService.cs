using ChurchData;
using ChurchData.DTOs;

namespace ChurchContracts
{

    public interface IFamilyMemberService
    {
        Task<ServiceResponse> SubmitFamilyMemberAsync(PendingFamilyMemberRequestDto requestDto);


        Task<ServiceResponse> ApproveFamilyMemberAsync(FamilyMemberApprovalDto approvalDto);
    }
}

