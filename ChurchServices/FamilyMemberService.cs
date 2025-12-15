using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Dapper.SqlMapper;

namespace ChurchServices
{
    public class FamilyMemberService : IFamilyMemberService
    {
        private readonly IFamilyMemberRepository _familyMemberRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionService> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FamilyMemberService(IFamilyMemberRepository familyMemberRepository, ApplicationDbContext context, ILogger<TransactionService> logger,
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
                SubmittedBy = userid,
                ActionType = "INSERT",
                SubmittedData = requestDto.Payload,  // Payload is of type JsonElement
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
            try
            {
                var (_, userParishId, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);
                var userid = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);
                // Call the stored procedure, no need to pass action
                int result = await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT ManageFamilyMemberApproval({approvalDto.ActionId}, {userid});"
                );

                return new ServiceResponse
                {
                    Success = true,
                    Message = $"Approval process completed successfully. Rows affected: {result}"
                };
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

            // Manual inline mapping from entity to DTO
            var dto = new FamilyMemberDto
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
                // Map Contacts (if any)
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
                // Map Identity if exists
                Identity = member.Identity == null ? null : new FamilyMemberIdentityDto
                {
                    IdentityId = member.Identity.IdentityId,
                    AadharNumber = member.Identity.AadharNumber,
                    PassportNumber = member.Identity.PassportNumber,
                    DrivingLicense = member.Identity.DrivingLicense,
                    VoterId = member.Identity.VoterId
                },
                // Map Occupation if exists
                Occupation = member.Occupation == null ? null : new FamilyMemberOccupationDto
                {
                    OccupationId = member.Occupation.OccupationId,
                    Qualification = member.Occupation.Qualification,
                    StudentOrEmployee = member.Occupation.StudentOrEmployee?.ToString(),
                    ClassOrWork = member.Occupation.ClassOrWork,
                    SchoolOrWorkplace = member.Occupation.SchoolOrWorkplace,
                    SundaySchoolClass = member.Occupation.SundaySchoolClass
                },
                // Map Sacraments if exists
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
                // Map Relations (if any)
                Relations = member.Relations?.Select(r => new FamilyMemberRelationsDto
                {
                    RelationId = r.RelationId,
                    FatherName = r.FatherName,
                    MotherName = r.MotherName,
                    SpouseId = r.SpouseId,
                    ParentId = r.ParentId
                }).ToList(),
                // Map Files if exists
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
                // Map Lifecycle if exists
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

            return new ServiceResponse<FamilyMemberDto>
            {
                Success = true,
                Data = dto,
                Message = "Family member retrieved successfully."
            };
        }
        public async Task<ServiceResponse<IEnumerable<FamilyMemberDto>>> GetFamilyMembersFilteredAsync(int parishId, int? familyId, FamilyMemberFilterRequest filterRequest)
        {

            //var (userRole, userParishId, userFamilyId) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);

            //if (parishId != userParishId)
            //{
            //    throw new UnauthorizedAccessException("You do not have permission to view directory for this parish.");
            //}


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

            //if (userRole == "FamilyMember")
            //{
            //    members = members.Where(m => m.FamilyId == userFamilyId).ToList();
            //}

            var dtos = members.Select(member => MapFamilyMemberToDto(member, units, families, filterRequest.Fields)).ToList();

            return new ServiceResponse<IEnumerable<FamilyMemberDto>>
            {
                Success = true,
                Data = dtos,
                Message = "Filtered family members retrieved successfully."
            };
        }

        // New method: Retrieve all family members without filtering.
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
            // Helper lambdas for field checks
            bool HasField(string fieldName) =>
                requestedFields.Contains(fieldName);

            bool HasFullObjectOrField(string objectName) =>
                requestedFields.Any(f => f == objectName || f.StartsWith(objectName + "."));

            // Always include these core properties
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
                // Always include the first contact’s mobile phone and the first relation’s parentId
                MobilePhone = member.Contacts?.FirstOrDefault()?.MobilePhone,
                ParentId = member.Relations?.FirstOrDefault()?.ParentId
            };

            // Scalar fields
            if (HasField("ParishId")) dto.ParishId = member.ParishId;
            if (HasField("UnitId")) dto.UnitId = member.UnitId;
            if (HasField("FirstName")) dto.FirstName = member.FirstName;
            if (HasField("LastName")) dto.LastName = member.LastName;
            if (HasField("Nickname")) dto.Nickname = member.Nickname;
            if (HasField("Gender")) dto.Gender = member.Gender.ToString();
            if (HasField("DateOfBirth")) dto.DateOfBirth = member.DateOfBirth;
            if (HasField("Age")) dto.Age = member.Age;
            if (HasField("MaritalStatus") && member.MaritalStatus.HasValue)
                dto.MaritalStatus = member.MaritalStatus.Value.ToString();
            if (HasField("ActiveMember")) dto.ActiveMember = member.ActiveMember;
            if (HasField("MemberStatus") && member.MemberStatus.HasValue)
                dto.MemberStatus = member.MemberStatus.Value.ToString();

            // Occupation
            if (HasFullObjectOrField("Occupation") && member.Occupation != null)
            {
                dto.Occupation = new FamilyMemberOccupationDto();
                if (HasField("Occupation.Qualification") || HasField("Occupation"))
                    dto.Occupation.Qualification = member.Occupation.Qualification;
                if (HasField("Occupation.StudentOrEmployee") || HasField("Occupation"))
                {
                    if (member.Occupation.StudentOrEmployee.HasValue)
                        dto.Occupation.StudentOrEmployee = member.Occupation.StudentOrEmployee.ToString();
                }
                if (HasField("Occupation.ClassOrWork") || HasField("Occupation"))
                    dto.Occupation.ClassOrWork = member.Occupation.ClassOrWork;
                if (HasField("Occupation.SchoolOrWorkplace") || HasField("Occupation"))
                    dto.Occupation.SchoolOrWorkplace = member.Occupation.SchoolOrWorkplace;
                if (HasField("Occupation.SundaySchoolClass") || HasField("Occupation"))
                    dto.Occupation.SundaySchoolClass = member.Occupation.SundaySchoolClass;
            }

            // Identity
            if (HasFullObjectOrField("Identity") && member.Identity != null)
            {
                dto.Identity = new FamilyMemberIdentityDto();
                if (HasField("Identity.AadharNumber") || HasField("Identity"))
                    dto.Identity.AadharNumber = member.Identity.AadharNumber;
                if (HasField("Identity.PassportNumber") || HasField("Identity"))
                    dto.Identity.PassportNumber = member.Identity.PassportNumber;
                if (HasField("Identity.DrivingLicense") || HasField("Identity"))
                    dto.Identity.DrivingLicense = member.Identity.DrivingLicense;
                if (HasField("Identity.VoterId") || HasField("Identity"))
                    dto.Identity.VoterId = member.Identity.VoterId;
            }

            // Sacraments
            if (HasFullObjectOrField("Sacraments") && member.Sacraments != null)
            {
                dto.Sacraments = new FamilyMemberSacramentsDto();
                if (HasField("Sacraments.BaptismalName") || HasField("Sacraments"))
                    dto.Sacraments.BaptismalName = member.Sacraments.BaptismalName;
                if (HasField("Sacraments.BaptismDate") || HasField("Sacraments"))
                    dto.Sacraments.BaptismDate = member.Sacraments.BaptismDate;
                if (HasField("Sacraments.MarriageDate") || HasField("Sacraments"))
                    dto.Sacraments.MarriageDate = member.Sacraments.MarriageDate;
                if (HasField("Sacraments.MooronDate") || HasField("Sacraments"))
                    dto.Sacraments.MooronDate = member.Sacraments.MooronDate;
                if (HasField("Sacraments.BaptismInOurChurch") || HasField("Sacraments"))
                    dto.Sacraments.BaptismInOurChurch = member.Sacraments.BaptismInOurChurch;
                if (HasField("Sacraments.MarriageInOurChurch") || HasField("Sacraments"))
                    dto.Sacraments.MarriageInOurChurch = member.Sacraments.MarriageInOurChurch;
                if (HasField("Sacraments.MooronInOurChurch") || HasField("Sacraments"))
                    dto.Sacraments.MooronInOurChurch = member.Sacraments.MooronInOurChurch;
            }

            // Contacts (collection)
            if (HasFullObjectOrField("Contacts") && member.Contacts != null)
            {
                foreach (var c in member.Contacts)
                {
                    var cd = new FamilyMemberContactsDto();
                    if (HasField("Contacts.AddressLine2") || HasField("Contacts")) cd.AddressLine2 = c.AddressLine2;
                    if (HasField("Contacts.AddressLine3") || HasField("Contacts")) cd.AddressLine3 = c.AddressLine3;
                    if (HasField("Contacts.PostOffice") || HasField("Contacts")) cd.PostOffice = c.PostOffice;
                    if (HasField("Contacts.PinCode") || HasField("Contacts")) cd.PinCode = c.PinCode;
                    if (HasField("Contacts.LandPhone") || HasField("Contacts")) cd.LandPhone = c.LandPhone;
                    if (HasField("Contacts.MobilePhone") || HasField("Contacts")) cd.MobilePhone = c.MobilePhone;
                    if (HasField("Contacts.Email") || HasField("Contacts")) cd.Email = c.Email;
                    if (HasField("Contacts.FacebookProfile") || HasField("Contacts")) cd.FacebookProfile = c.FacebookProfile;
                    if (HasField("Contacts.GeoLocation") || HasField("Contacts")) cd.GeoLocation = c.GeoLocation;
                    dto.Contacts.Add(cd);
                }
            }

            // Relations (collection)
            if (HasFullObjectOrField("Relations") && member.Relations != null)
            {
                foreach (var r in member.Relations)
                {
                    var rd = new FamilyMemberRelationsDto();
                    if (HasField("Relations.FatherName") || HasField("Relations")) rd.FatherName = r.FatherName;
                    if (HasField("Relations.MotherName") || HasField("Relations")) rd.MotherName = r.MotherName;
                    if (HasField("Relations.SpouseId") || HasField("Relations")) rd.SpouseId = r.SpouseId;
                    if (HasField("Relations.ParentId") || HasField("Relations")) rd.ParentId = r.ParentId;
                    dto.Relations.Add(rd);
                }
            }

            // Files
            if (HasFullObjectOrField("Files") && member.Files != null)
            {
                dto.Files = new FamilyMemberFilesDto();
                if (HasField("Files.MarriageFileNo") || HasField("Files")) dto.Files.MarriageFileNo = member.Files.MarriageFileNo;
                if (HasField("Files.BaptismFileNo") || HasField("Files")) dto.Files.BaptismFileNo = member.Files.BaptismFileNo;
                if (HasField("Files.DeathFileNo") || HasField("Files")) dto.Files.DeathFileNo = member.Files.DeathFileNo;
                if (HasField("Files.JoinFileNo") || HasField("Files")) dto.Files.JoinFileNo = member.Files.JoinFileNo;
                if (HasField("Files.MooronFileNo") || HasField("Files")) dto.Files.MooronFileNo = member.Files.MooronFileNo;
                if (HasField("Files.CommonCellNo") || HasField("Files")) dto.Files.CommonCellNo = member.Files.CommonCellNo;
            }

            // Lifecycle
            if (HasFullObjectOrField("Lifecycle") && member.Lifecycle != null)
            {
                dto.Lifecycle = new FamilyMemberLifecycleDto();
                if (HasField("Lifecycle.CommonCell") || HasField("Lifecycle")) dto.Lifecycle.CommonCell = member.Lifecycle.CommonCell;
                if (HasField("Lifecycle.LeftReason") || HasField("Lifecycle")) dto.Lifecycle.LeftReason = member.Lifecycle.LeftReason;
                if (HasField("Lifecycle.JoinDate") || HasField("Lifecycle")) dto.Lifecycle.JoinDate = member.Lifecycle.JoinDate;
                if (HasField("Lifecycle.LeftDate") || HasField("Lifecycle")) dto.Lifecycle.LeftDate = member.Lifecycle.LeftDate;
                if (HasField("Lifecycle.BurialPlace") || HasField("Lifecycle")) dto.Lifecycle.BurialPlace = member.Lifecycle.BurialPlace?.ToString();
                if (HasField("Lifecycle.DeathDate") || HasField("Lifecycle")) dto.Lifecycle.DeathDate = member.Lifecycle.DeathDate;
            }

            return dto;
        }

        public async Task<UserInfo> ValidateUserWithMobile(string mobilenumber)
        {
            if (string.IsNullOrWhiteSpace(mobilenumber))
            {
                return null; // Handle invalid input
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
                return null; // No family member found with the given mobile number
            }

            return new UserInfo
            {
                ParishId = familyMember.ParishId,
                FamilyId = familyMember.FamilyId,
                FamilyNumber = familyMember.FamilyNumber,
                FamilyName =  "", 
                FirstName = familyMember.FirstName
            };
        }
        // Helper method to ensure dto.Occupation is initialized


    }
}
