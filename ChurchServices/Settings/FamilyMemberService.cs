using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchServices.Settings
{
    public class FamilyMemberService : IFamilyMemberService
    {
        private readonly IFamilyMemberRepository _familyMemberRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FamilyMemberService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FamilyMemberService(IFamilyMemberRepository familyMemberRepository, ApplicationDbContext context, ILogger<FamilyMemberService> logger,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _familyMemberRepository = familyMemberRepository;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }


        public async Task<ServiceResponse> SubmitFamilyMemberAsync(PendingFamilyMemberRequestDto requestDto)
        {
            var (_, userParishId, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);
            var userid = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);
            var pendingAction = new PendingFamilyMemberAction
            {
                FamilyId = requestDto.FamilyId,
                ParishId = requestDto.ParishId,
                MemberId=requestDto.Payload.GetProperty("MemberId").GetInt32(),
                SubmittedBy = userid,
                ActionType = requestDto.ActionType,
                SubmittedData = requestDto.Payload,
                ApprovalStatus = "Pending",
                SubmittedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
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
            try
            {
                var (_, userParishId, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);
                var userid = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);

                var pendingAction = await _familyMemberRepository.GetPendingActionByIdAsync(approvalDto.ActionId);
                if (pendingAction == null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "Pending action not found or already processed."
                    };
                }

                if (pendingAction.ParishId != userParishId)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "Unauthorized: You cannot approve actions from another parish."
                    };
                }

                if (approvalDto.ApprovalStatus == "Approved")
                {
                    int result = await _context.Database.ExecuteSqlInterpolatedAsync(
                        $"SELECT ManageFamilyMemberApproval({approvalDto.ActionId}, {userid});"
                    );

                    return new ServiceResponse
                    {
                        Success = true,
                        Message = $"Family member approved successfully. Rows affected: {result}"
                    };
                }
                else if (approvalDto.ApprovalStatus == "Rejected")
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE pending_family_member_actions 
                           SET approval_status = 'Rejected', 
                               approved_by = {userid}, 
                               approved_at = NOW() 
                           WHERE action_id = {approvalDto.ActionId}"
                    );

                    return new ServiceResponse
                    {
                        Success = true,
                        Message = "Family member request rejected successfully."
                    };
                }
                else
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "Invalid approval status. Must be 'Approved' or 'Rejected'.",
                    };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<PendingFamilyMemberApprovalListDto>>> GetPendingApprovalListAsync(int parishId)
        {
            try
            {
                var pendingActions = await _familyMemberRepository.GetPendingApprovalListAsync(parishId);

                if (pendingActions == null || !pendingActions.Any())
                {
                    return new ServiceResponse<IEnumerable<PendingFamilyMemberApprovalListDto>>
                    {
                        Success = true,
                        Data = new List<PendingFamilyMemberApprovalListDto>(),
                        Message = "No pending approvals found."
                    };
                }

                var dtos = pendingActions.Select(action => new PendingFamilyMemberApprovalListDto
                {
                    ActionId = action.ActionId,
                    MemberId = action.MemberId,
                    ActionType = action.ActionType,
                    FamilyId = action.FamilyId,
                    ParishId = action.ParishId,
                    SubmittedBy = action.SubmittedBy,
                    SubmittedData = action.SubmittedData,
                    SubmittedAt = action.SubmittedAt
                }).ToList();

                return new ServiceResponse<IEnumerable<PendingFamilyMemberApprovalListDto>>
                {
                    Success = true,
                    Data = dtos,
                    Message = "Pending approval list retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<PendingFamilyMemberApprovalListDto>>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<FamilyMemberDto>> GetFamilyMemberByIdAsync(int memberId)
        {
            var member = await _familyMemberRepository.GetFamilyMemberByIdAsync(memberId);
            if (member == null)
            {
                return new ServiceResponse<FamilyMemberDto>
                {
                    Success = false,
                    Message = "Family member not found."
                };
            }

            var dto = MapFamilyMemberToDto(member);

            return new ServiceResponse<FamilyMemberDto>
            {
                Success = true,
                Data = dto,
                Message = "Family member retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<FamilyMemberDto>>> GetFamilyMembersFilteredAsync(int parishId, int? familyId, FamilyMemberFilterRequest filterRequest)
        {
            var units = await _context.Units
                     .Where(u => u.ParishId == parishId)
                     .ToListAsync();

            var families = await _context.Families
                    .Where(f => f.ParishId == parishId)
                    .ToListAsync();

            var members = await _familyMemberRepository.GetFamilyMembersFilteredAsync(parishId, familyId, filterRequest);

            if (members == null || !members.Any())
            {
                return new ServiceResponse<IEnumerable<FamilyMemberDto>>
                {
                    Success = false,
                    Message = "No family members found matching the filter criteria."
                };
            }
            if (familyId != null && familyId != 0)
            {
                members = members.Where(m => m.FamilyNumber == familyId).ToList();
            }

            var dtos = members.Select(member => MapFamilyMemberToDto(member, units, families, filterRequest.Fields)).ToList();

            return new ServiceResponse<IEnumerable<FamilyMemberDto>>
            {
                Success = true,
                Data = dtos,
                Message = "Filtered family members retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<FamilyMemberDto>>> GetAllFamilyMembersAsync(int? parishId, int? familyId)
        {
            var members = await _familyMemberRepository.GetAllFamilyMembersAsync(parishId, familyId);
            var dtos = members.Select(member => MapFamilyMemberToDto(member)).ToList();
            return new ServiceResponse<IEnumerable<FamilyMemberDto>>
            {
                Success = true,
                Data = dtos,
                Message = "All family members retrieved successfully."
            };
        }

        private FamilyMemberDto MapFamilyMemberToDto(FamilyMember member)
        {
            return new FamilyMemberDto
            {
                MemberId = member.MemberId,
                FamilyId = member.FamilyId,
                ParishId = member.ParishId ?? 0,
                UnitId = member.UnitId ?? 0,
                FamilyNumber = member.FamilyNumber,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Nickname = member.Nickname,
                Gender = member.Gender.ToString(),
                DateOfBirth = member.DateOfBirth,
                MaritalStatus = member.MaritalStatus?.ToString(),
                ActiveMember = member.ActiveMember,
                MemberStatus = member.MemberStatus?.ToString(),
                CreatedAt = member.CreatedAt,
                UpdatedAt = member.UpdatedAt,
                Contacts = member.Contacts?.Select(c => new FamilyMemberContactsDto
                {
                    ContactId = c.ContactId,
                    AddressLine2 = c.AddressLine2,
                    AddressLine3 = c.AddressLine3,
                    PostOffice = c.PostOffice,
                    PinCode = c.PinCode,
                    LandPhone = c.LandPhone,
                    MobilePhone = c.MobilePhone,
                    Email = c.Email,
                    FacebookProfile = c.FacebookProfile,
                    GeoLocation = c.GeoLocation
                }).ToList(),
                Identity = member.Identity == null ? null : new FamilyMemberIdentityDto
                {
                    IdentityId = member.Identity.IdentityId,
                    AadharNumber = member.Identity.AadharNumber,
                    PassportNumber = member.Identity.PassportNumber,
                    DrivingLicense = member.Identity.DrivingLicense,
                    VoterId = member.Identity.VoterId
                },
                Occupation = member.Occupation == null ? null : new FamilyMemberOccupationDto
                {
                    OccupationId = member.Occupation.OccupationId,
                    Qualification = member.Occupation.Qualification,
                    StudentOrEmployee = member.Occupation.StudentOrEmployee?.ToString(),
                    ClassOrWork = member.Occupation.ClassOrWork,
                    SchoolOrWorkplace = member.Occupation.SchoolOrWorkplace,
                    SundaySchoolClass = member.Occupation.SundaySchoolClass
                },
                Sacraments = member.Sacraments == null ? null : new FamilyMemberSacramentsDto
                {
                    SacramentId = member.Sacraments.SacramentId,
                    BaptismalName = member.Sacraments.BaptismalName,
                    BaptismDate = member.Sacraments.BaptismDate,
                    MarriageDate = member.Sacraments.MarriageDate,
                    MooronDate = member.Sacraments.MooronDate,
                    MooronInOurChurch = member.Sacraments.MooronInOurChurch,
                    MarriageInOurChurch = member.Sacraments.MarriageInOurChurch,
                    BaptismInOurChurch = member.Sacraments.BaptismInOurChurch
                },
                Relations = member.Relations?.Select(r => new FamilyMemberRelationsDto
                {
                    RelationId = r.RelationId,
                    FatherName = r.FatherName,
                    MotherName = r.MotherName,
                    SpouseId = r.SpouseId,
                    ParentId = r.ParentId
                }).ToList(),
                Files = member.Files == null ? null : new FamilyMemberFilesDto
                {
                    FileId = member.Files.FileId,
                    MarriageFileNo = member.Files.MarriageFileNo,
                    BaptismFileNo = member.Files.BaptismFileNo,
                    DeathFileNo = member.Files.DeathFileNo,
                    JoinFileNo = member.Files.JoinFileNo,
                    MooronFileNo = member.Files.MooronFileNo,
                    CommonCellNo = member.Files.CommonCellNo
                },
                Lifecycle = member.Lifecycle == null ? null : new FamilyMemberLifecycleDto
                {
                    LifecycleId = member.Lifecycle.LifecycleId,
                    CommonCell = member.Lifecycle.CommonCell,
                    LeftReason = member.Lifecycle.LeftReason,
                    JoinDate = member.Lifecycle.JoinDate,
                    LeftDate = member.Lifecycle.LeftDate,
                    BurialPlace = member.Lifecycle.BurialPlace?.ToString(),
                    DeathDate = member.Lifecycle.DeathDate
                }
            };
        }

        private FamilyMemberDto MapFamilyMemberToDto(FamilyMember member, List<Unit> units, List<Family> families, List<string> requestedFields)
        {
            bool HasField(string fieldName) =>
                requestedFields.Contains(fieldName);

            bool HasFullObjectOrField(string objectName) =>
                requestedFields.Any(f => f == objectName || f.StartsWith(objectName + "."));

            var dto = new FamilyMemberDto
            {
                MemberId = member.MemberId,
                FamilyId = member.FamilyId,
                FamilyNumber = member.FamilyNumber,
                ParishId = member.ParishId ?? 0,
                UnitId = member.UnitId ?? 0,
                UnitName = units.Where(w => w.UnitId == member.UnitId).Select(s => s.UnitName).FirstOrDefault(),
                FamilyName = families.Where(w => w.FamilyId == member.FamilyId).Select(s => $"{s.HeadName} {s.FamilyName}").FirstOrDefault(),
                FirstName = member.FirstName,
                LastName = member.LastName,
                Gender = member.Gender.ToString(),
                Age = member.Age,
                MemberStatus = member.MemberStatus?.ToString(),
                MaritalStatus = member.MaritalStatus?.ToString(),
                MobilePhone = member.Contacts?.FirstOrDefault()?.MobilePhone,
                ParentId = member.Relations?.FirstOrDefault()?.ParentId
            };

            if (HasField("Nickname")) dto.Nickname = member.Nickname;
            if (HasField("DateOfBirth")) dto.DateOfBirth = member.DateOfBirth;
            if (HasField("ActiveMember")) dto.ActiveMember = member.ActiveMember;

            if (HasFullObjectOrField("Occupation") && member.Occupation != null)
            {
                dto.Occupation = new FamilyMemberOccupationDto
                {
                    Qualification = member.Occupation.Qualification,
                    StudentOrEmployee = member.Occupation.StudentOrEmployee?.ToString(),
                    ClassOrWork = member.Occupation.ClassOrWork,
                    SchoolOrWorkplace = member.Occupation.SchoolOrWorkplace,
                    SundaySchoolClass = member.Occupation.SundaySchoolClass
                };
            }

            if (HasFullObjectOrField("Identity") && member.Identity != null)
            {
                dto.Identity = new FamilyMemberIdentityDto
                {
                    AadharNumber = member.Identity.AadharNumber,
                    PassportNumber = member.Identity.PassportNumber,
                    DrivingLicense = member.Identity.DrivingLicense,
                    VoterId = member.Identity.VoterId
                };
            }

            if (HasFullObjectOrField("Sacraments") && member.Sacraments != null)
            {
                dto.Sacraments = new FamilyMemberSacramentsDto
                {
                    BaptismalName = member.Sacraments.BaptismalName,
                    BaptismDate = member.Sacraments.BaptismDate,
                    MarriageDate = member.Sacraments.MarriageDate,
                    MooronDate = member.Sacraments.MooronDate,
                    BaptismInOurChurch = member.Sacraments.BaptismInOurChurch,
                    MarriageInOurChurch = member.Sacraments.MarriageInOurChurch,
                    MooronInOurChurch = member.Sacraments.MooronInOurChurch
                };
            }

            if (HasFullObjectOrField("Contacts") && member.Contacts != null)
            {
                dto.Contacts = member.Contacts.Select(c => new FamilyMemberContactsDto
                {
                    ContactId = c.ContactId,
                    AddressLine2 = c.AddressLine2,
                    AddressLine3 = c.AddressLine3,
                    PostOffice = c.PostOffice,
                    PinCode = c.PinCode,
                    LandPhone = c.LandPhone,
                    MobilePhone = c.MobilePhone,
                    Email = c.Email,
                    FacebookProfile = c.FacebookProfile,
                    GeoLocation = c.GeoLocation
                }).ToList();
            }

            if (HasFullObjectOrField("Relations") && member.Relations != null)
            {
                dto.Relations = member.Relations.Select(r => new FamilyMemberRelationsDto
                {
                    RelationId = r.RelationId,
                    FatherName = r.FatherName,
                    MotherName = r.MotherName,
                    SpouseId = r.SpouseId,
                    ParentId = r.ParentId
                }).ToList();
            }

            if (HasFullObjectOrField("Files") && member.Files != null)
            {
                dto.Files = new FamilyMemberFilesDto
                {
                    FileId = member.Files.FileId,
                    MarriageFileNo = member.Files.MarriageFileNo,
                    BaptismFileNo = member.Files.BaptismFileNo,
                    DeathFileNo = member.Files.DeathFileNo,
                    JoinFileNo = member.Files.JoinFileNo,
                    MooronFileNo = member.Files.MooronFileNo,
                    CommonCellNo = member.Files.CommonCellNo
                };
            }

            if (HasFullObjectOrField("Lifecycle") && member.Lifecycle != null)
            {
                dto.Lifecycle = new FamilyMemberLifecycleDto
                {
                    LifecycleId = member.Lifecycle.LifecycleId,
                    CommonCell = member.Lifecycle.CommonCell,
                    LeftReason = member.Lifecycle.LeftReason,
                    JoinDate = member.Lifecycle.JoinDate,
                    LeftDate = member.Lifecycle.LeftDate,
                    BurialPlace = member.Lifecycle.BurialPlace?.ToString(),
                    DeathDate = member.Lifecycle.DeathDate
                };
            }

            return dto;
        }

        public async Task<UserInfo> ValidateUserWithMobile(string mobilenumber)
        {
            if (string.IsNullOrWhiteSpace(mobilenumber))
            {
                return null;
            }
            if (mobilenumber.StartsWith("91"))
            {
                mobilenumber = mobilenumber.Substring(2);
            }
            var familyMember = _context.FamilyMembers
                .Include(fm => fm.Contacts)
                .FirstOrDefault(fm => fm.Contacts.Any(c => c.MobilePhone == mobilenumber));

            if (familyMember == null)
            {
                return null;
            }

            return new UserInfo
            {
                ParishId = familyMember.ParishId,
                FamilyId = familyMember.FamilyId,
                FamilyNumber = familyMember.FamilyNumber,
                FamilyName = "",
                FirstName = familyMember.FirstName
            };
        }
    }
}
