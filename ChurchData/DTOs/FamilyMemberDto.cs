using System;
using System.Collections.Generic;

namespace ChurchData.DTOs
{
    public class FamilyMemberDto
    {
        public int MemberId { get; set; }
        public int FamilyId { get; set; }
        public int? ParishId { get; set; }
        public int? UnitId { get; set; }
        public int FamilyNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nickname { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string MaritalStatus { get; set; }
        public bool ActiveMember { get; set; }
        public string MemberStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Nested collections / related objects
        public ICollection<FamilyMemberContactsDto> Contacts { get; set; } = new List<FamilyMemberContactsDto>();
        public FamilyMemberIdentityDto Identity { get; set; }
        public FamilyMemberOccupationDto Occupation { get; set; }
        public FamilyMemberSacramentsDto Sacraments { get; set; }
        public ICollection<FamilyMemberRelationsDto> Relations { get; set; } = new List<FamilyMemberRelationsDto>();
        public FamilyMemberFilesDto Files { get; set; }
        public FamilyMemberLifecycleDto Lifecycle { get; set; }
    }

    public class FamilyMemberContactsDto
    {
        public int ContactId { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string PostOffice { get; set; }
        public string PinCode { get; set; }
        public string LandPhone { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public string FacebookProfile { get; set; }
        public string GeoLocation { get; set; }
    }

    public class FamilyMemberIdentityDto
    {
        public int IdentityId { get; set; }
        public string AadharNumber { get; set; }
        public string PassportNumber { get; set; }
        public string DrivingLicense { get; set; }
        public string VoterId { get; set; }
    }

    public class FamilyMemberOccupationDto
    {
        public int OccupationId { get; set; }
        public string Qualification { get; set; }
        public string StudentOrEmployee { get; set; }
        public string ClassOrWork { get; set; }
        public string SchoolOrWorkplace { get; set; }
        public string SundaySchoolClass { get; set; }
    }

    public class FamilyMemberSacramentsDto
    {
        public int SacramentId { get; set; }
        public string BaptismalName { get; set; }
        public DateTime? BaptismDate { get; set; }
        public DateTime? MarriageDate { get; set; }
        public DateTime? MooronDate { get; set; }
        public bool MooronInOurChurch { get; set; }
        public bool MarriageInOurChurch { get; set; }
        public bool BaptismInOurChurch { get; set; }
    }

    public class FamilyMemberRelationsDto
    {
        public int RelationId { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public int? SpouseId { get; set; }
        public int? ParentId { get; set; }
    }

    public class FamilyMemberFilesDto
    {
        public int FileId { get; set; }
        public string MarriageFileNo { get; set; }
        public string BaptismFileNo { get; set; }
        public string DeathFileNo { get; set; }
        public string JoinFileNo { get; set; }
        public string MooronFileNo { get; set; }
        public string CommonCellNo { get; set; }
    }

    public class FamilyMemberLifecycleDto
    {
        public int LifecycleId { get; set; }
        public bool CommonCell { get; set; }
        public string LeftReason { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? LeftDate { get; set; }
        public string BurialPlace { get; set; }
        public DateTime? DeathDate { get; set; }
    }
}
