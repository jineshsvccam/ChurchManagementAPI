using System.Text.Json;
using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ChurchServices
{
    public class FamilyMemberService : IFamilyMemberService
    {
        private readonly IFamilyMemberRepository _familyMemberRepository;
        private readonly ApplicationDbContext _context;

        public FamilyMemberService(IFamilyMemberRepository familyMemberRepository, ApplicationDbContext context)
        {

            _familyMemberRepository = familyMemberRepository;
            _context = context;
        }

        public async Task<ServiceResponse> SubmitFamilyMemberAsync(PendingFamilyMemberRequestDto requestDto)
        {
            var pendingAction = new PendingFamilyMemberAction
            {
                FamilyId = requestDto.FamilyId,
                ParishId = requestDto.ParishId,               
                SubmittedBy = requestDto.SubmittedBy,
                ActionType = "INSERT",
                SubmittedData = requestDto.Payload,
                ApprovalStatus = "Pending",
                SubmittedAt = DateTime.UtcNow
            };

            await _familyMemberRepository.AddPendingActionAsync(pendingAction);

            return new ServiceResponse
            {
                Success = true,
                Message = "Family member submitted for approval."
            };
        }

        public async Task<ServiceResponse> ApproveFamilyMemberAsync(FamilyMemberApprovalDto approvalDto)
        {
            // Call the stored procedure to approve the pending action.
            var result = await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT ApprovePendingFamilyMember({approvalDto.ActionId}, {approvalDto.ApprovedBy});"
            );

            return new ServiceResponse
            {
                Success = true,
                Message = $"Family member approved and inserted via stored procedure. Rows affected: {result}"
            };
        }
    }
}
