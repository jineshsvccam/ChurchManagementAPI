using ChurchDTOs.DTOs.Entities;

namespace ChurchContracts
{
    public interface IFamilyMemberService
    {
        // Submission & Approval
        Task<ServiceResponse> SubmitFamilyMemberAsync(PendingFamilyMemberRequestDto requestDto);
        Task<ServiceResponse> ApproveFamilyMemberAsync(FamilyMemberApprovalDto approvalDto);
        Task<ServiceResponse<IEnumerable<PendingFamilyMemberApprovalListDto>>> GetPendingApprovalListAsync(int parishId);

        // Data retrieval endpoints
        Task<ServiceResponse<FamilyMemberDto>> GetFamilyMemberByIdAsync(int memberId);
        Task<ServiceResponse<IEnumerable<FamilyMemberDto>>> GetFamilyMembersFilteredAsync(int parishId, int? familyId,  FamilyMemberFilterRequest filterRequest);
        Task<ServiceResponse<IEnumerable<FamilyMemberDto>>> GetFamilyMembersFilteredAsync(int parishId, int? familyId, FamilyMemberFilterRequest filterRequest, string userRole, int? userFamilyId);

        // New method for retrieving all family members
        Task<ServiceResponse<IEnumerable<FamilyMemberDto>>> GetAllFamilyMembersAsync(int? parishId, int? familyId);
        Task<ServiceResponse<IEnumerable<FamilyMemberDto>>> GetAllFamilyMembersAsync(int? parishId, int? familyId, string userRole, int? userFamilyId);
        
        Task<UserInfo> ValidateUserWithMobile(string mobilenumber);

        // Mobile number view with rate limiting (max 5 per day)
        Task<ServiceResponse<MemberMobileResponseDto>> GetMemberMobileNumberAsync(int memberId, Guid userId);
    }
}
