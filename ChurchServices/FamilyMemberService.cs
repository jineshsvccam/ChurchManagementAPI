using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<ServiceResponse<IEnumerable<FamilyMemberDto>>> GetFamilyMembersFilteredAsync(FamilyMemberFilterRequest filterRequest)
        {
            var members = await _familyMemberRepository.GetFamilyMembersFilteredAsync(filterRequest);
            var dtos = members.Select(member => MapFamilyMemberToDto(member, filterRequest.Fields)).ToList();

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
        private FamilyMemberDto MapFamilyMemberToDto(FamilyMember member, List<string> requestedFields)
        {
            // Always include these core properties
            var dto = new FamilyMemberDto
            {
                MemberId = member.MemberId,
                FamilyId = member.FamilyId,
                FamilyNumber = member.FamilyNumber,
                CreatedAt = member.CreatedAt,
                UpdatedAt = member.UpdatedAt
            };

            foreach (var field in requestedFields)
            {
                switch (field)
                {
                    case "MemberId":
                        dto.MemberId = member.MemberId;
                        break;
                    case "FamilyId":
                        dto.FamilyId = member.FamilyId;
                        break;
                    case "ParishId":
                        dto.ParishId = member.ParishId ?? 0;
                        break;
                    case "UnitId":
                        dto.UnitId = member.UnitId ?? 0;
                        break;
                    case "FamilyNumber":
                        dto.FamilyNumber = member.FamilyNumber;
                        break;
                    case "FirstName":
                        dto.FirstName = member.FirstName;
                        break;
                    case "LastName":
                        dto.LastName = member.LastName;
                        break;
                    case "Nickname":
                        dto.Nickname = member.Nickname;
                        break;
                    case "Gender":
                        dto.Gender = member.Gender.ToString();
                        break;
                    case "DateOfBirth":
                        dto.DateOfBirth = member.DateOfBirth;
                        break;
                    case "MaritalStatus":
                        dto.MaritalStatus = member.MaritalStatus?.ToString();
                        break;
                    case "ActiveMember":
                        dto.ActiveMember = member.ActiveMember;
                        break;
                    case "MemberStatus":
                        dto.MemberStatus = member.MemberStatus?.ToString();
                        break;
                    case "CreatedAt":
                        dto.CreatedAt = member.CreatedAt;
                        break;
                    case "UpdatedAt":
                        dto.UpdatedAt = member.UpdatedAt;
                        break;
                    // Nested mapping for Occupation
                    case "Occupation.Qualification":
                        EnsureOccupation(dto).Qualification = member.Occupation?.Qualification;
                        break;
                    case "Occupation.StudentOrEmployee":
                        EnsureOccupation(dto).StudentOrEmployee = member.Occupation?.StudentOrEmployee?.ToString();
                        break;
                    case "Occupation.ClassOrWork":
                        EnsureOccupation(dto).ClassOrWork = member.Occupation?.ClassOrWork;
                        break;
                    case "Occupation.SchoolOrWorkplace":
                        EnsureOccupation(dto).SchoolOrWorkplace = member.Occupation?.SchoolOrWorkplace;
                        break;
                    case "Occupation.SundaySchoolClass":
                        EnsureOccupation(dto).SundaySchoolClass = member.Occupation?.SundaySchoolClass;
                        break;
                    // Nested mapping for Identity
                    case "Identity.AadharNumber":
                        if (member.Identity != null)
                        {
                            dto.Identity = dto.Identity ?? new FamilyMemberIdentityDto();
                            dto.Identity.AadharNumber = member.Identity.AadharNumber;
                        }
                        break;
                    case "Identity.PassportNumber":
                        if (member.Identity != null)
                        {
                            dto.Identity = dto.Identity ?? new FamilyMemberIdentityDto();
                            dto.Identity.PassportNumber = member.Identity.PassportNumber;
                        }
                        break;
                    case "Identity.DrivingLicense":
                        if (member.Identity != null)
                        {
                            dto.Identity = dto.Identity ?? new FamilyMemberIdentityDto();
                            dto.Identity.DrivingLicense = member.Identity.DrivingLicense;
                        }
                        break;
                    case "Identity.VoterId":
                        if (member.Identity != null)
                        {
                            dto.Identity = dto.Identity ?? new FamilyMemberIdentityDto();
                            dto.Identity.VoterId = member.Identity.VoterId;
                        }
                        break;
                    // Nested mapping for Sacraments
                    case "Sacraments.BaptismalName":
                        if (member.Sacraments != null)
                        {
                            dto.Sacraments = dto.Sacraments ?? new FamilyMemberSacramentsDto();
                            dto.Sacraments.BaptismalName = member.Sacraments.BaptismalName;
                        }
                        break;
                    case "Sacraments.BaptismDate":
                        if (member.Sacraments != null)
                        {
                            dto.Sacraments = dto.Sacraments ?? new FamilyMemberSacramentsDto();
                            dto.Sacraments.BaptismDate = member.Sacraments.BaptismDate;
                        }
                        break;
                    case "Sacraments.MarriageDate":
                        if (member.Sacraments != null)
                        {
                            dto.Sacraments = dto.Sacraments ?? new FamilyMemberSacramentsDto();
                            dto.Sacraments.MarriageDate = member.Sacraments.MarriageDate;
                        }
                        break;
                    case "Sacraments.MooronDate":
                        if (member.Sacraments != null)
                        {
                            dto.Sacraments = dto.Sacraments ?? new FamilyMemberSacramentsDto();
                            dto.Sacraments.MooronDate = member.Sacraments.MooronDate;
                        }
                        break;
                    case "Sacraments.MooronInOurChurch":
                        if (member.Sacraments != null)
                        {
                            dto.Sacraments = dto.Sacraments ?? new FamilyMemberSacramentsDto();
                            dto.Sacraments.MooronInOurChurch = member.Sacraments.MooronInOurChurch;
                        }
                        break;
                    case "Sacraments.MarriageInOurChurch":
                        if (member.Sacraments != null)
                        {
                            dto.Sacraments = dto.Sacraments ?? new FamilyMemberSacramentsDto();
                            dto.Sacraments.MarriageInOurChurch = member.Sacraments.MarriageInOurChurch;
                        }
                        break;
                    case "Sacraments.BaptismInOurChurch":
                        if (member.Sacraments != null)
                        {
                            dto.Sacraments = dto.Sacraments ?? new FamilyMemberSacramentsDto();
                            dto.Sacraments.BaptismInOurChurch = member.Sacraments.BaptismInOurChurch;
                        }
                        break;
                    // Nested mapping for Relations - assuming you want a flat list of relations as DTOs
                    case "Relations":
                        if (member.Relations != null)
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
                        break;
                    // Nested mapping for Files
                    case "Files":
                        if (member.Files != null)
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
                        break;
                    // Nested mapping for Lifecycle
                    case "Lifecycle":
                        if (member.Lifecycle != null)
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
                        break;
                }
            }
            return dto;
        }

        // Helper method to ensure dto.Occupation is initialized
        private FamilyMemberOccupationDto EnsureOccupation(FamilyMemberDto dto)
        {
            if (dto.Occupation == null)
            {
                dto.Occupation = new FamilyMemberOccupationDto();
            }
            return dto.Occupation;
        }

    }
}
