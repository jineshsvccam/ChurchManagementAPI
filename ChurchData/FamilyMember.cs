using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ChurchData
{
    public class FamilyMember
    {
        [NotMapped] // Ensure this field is not mapped to the database
        public string Action { get; set; } // INSERT or UPDATE
        [Key]
        public int MemberId { get; set; }
        public int FamilyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ContactInfo { get; set; }
        public string Role { get; set; }

        [JsonIgnore]
        public Family? Family { get; set; }
    }

  
    public class FamilyMemberN
    {
        [Key]
        public int MemberId { get; set; }

        [Required]
        public int FamilyId { get; set; }

        public int? ParishId { get; set; }
        public int? UnitId { get; set; } 

        [Required]
        public int FamilyNumber { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string Nickname { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [NotMapped]
        public int? Age { get; set; }

        public MaritalStatus? MaritalStatus { get; set; }

        public bool ActiveMember { get; set; } = true;

        public MemberStatus? MemberStatus { get; set; }

        public int? UpdatedUser { get; set; }

        public virtual User UpdatedUserNavigation { get; set; } // Navigation Property
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // One-to-Many: A Family Member can have multiple contacts
        public virtual ICollection<FamilyMemberContacts> Contacts { get; set; } = new List<FamilyMemberContacts>();

        // One-to-Many: A Family Member can have multiple relationships
        public virtual ICollection<FamilyMemberRelations> Relations { get; set; } = new List<FamilyMemberRelations>();

        // One-to-One Relationships
        public virtual FamilyMemberIdentity Identity { get; set; }
        public virtual FamilyMemberOccupation Occupation { get; set; }
        public virtual FamilyMemberSacraments Sacraments { get; set; }
        public virtual FamilyMemberFiles Files { get; set; }
        public virtual FamilyMemberLifecycle Lifecycle { get; set; }
    }

    // One-to-Many: A Family Member can have multiple contacts
    public class FamilyMemberContacts
    {
        [Key]
        public int ContactId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [MaxLength(255)]
        public string AddressLine2 { get; set; }

        [MaxLength(255)]
        public string AddressLine3 { get; set; }

        [MaxLength(100)]
        public string PostOffice { get; set; }

        [MaxLength(10)]
        public string PinCode { get; set; }

        [MaxLength(20)]
        public string LandPhone { get; set; }

        [MaxLength(20)]
        public string MobilePhone { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(255)]
        public string FacebookProfile { get; set; }

        [MaxLength(255)]
        public string GeoLocation { get; set; }

        [ForeignKey("MemberId")]
        public virtual FamilyMember Member { get; set; }
    }

    // One-to-Many: A Family Member can have multiple relations
    public class FamilyMemberRelations
    {
        [Key]
        public int RelationId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [MaxLength(100)]
        public string FatherName { get; set; }

        [MaxLength(100)]
        public string MotherName { get; set; }

        public int? SpouseId { get; set; }
        public int? ParentId { get; set; }

        [ForeignKey("MemberId")]
        public virtual FamilyMember Member { get; set; }

        [ForeignKey("SpouseId")]
        public virtual FamilyMember Spouse { get; set; }

        [ForeignKey("ParentId")]
        public virtual FamilyMember Parent { get; set; }
    }

    // One-to-One: Identity Documents
    public class FamilyMemberIdentity
    {
        [Key]
        public int IdentityId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [MaxLength(20)]
        public string AadharNumber { get; set; }

        [MaxLength(20)]
        public string PassportNumber { get; set; }

        [MaxLength(20)]
        public string DrivingLicense { get; set; }

        [MaxLength(20)]
        public string VoterId { get; set; }

        [ForeignKey("MemberId")]
        public virtual FamilyMember Member { get; set; }
    }

    // One-to-One: Occupation & Education
    public class FamilyMemberOccupation
    {
        [Key]
        public int OccupationId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [MaxLength(255)]
        public string Qualification { get; set; }

        public StudentOrEmployee? StudentOrEmployee { get; set; }

        [MaxLength(255)]
        public string ClassOrWork { get; set; }

        [MaxLength(255)]
        public string SchoolOrWorkplace { get; set; }

        [MaxLength(10)]
        public string SundaySchoolClass { get; set; }

        [ForeignKey("MemberId")]
        public virtual FamilyMember Member { get; set; }
    }

    // One-to-One: Sacraments
    public class FamilyMemberSacraments
    {
        [Key]
        public int SacramentId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [MaxLength(255)]
        public string BaptismalName { get; set; }

        public DateTime? BaptismDate { get; set; }
        public DateTime? MarriageDate { get; set; }
        public DateTime? MooronDate { get; set; }

        public bool MooronInOurChurch { get; set; } = false;
        public bool MarriageInOurChurch { get; set; } = false;
        public bool BaptismInOurChurch { get; set; } = false;

        [ForeignKey("MemberId")]
        public virtual FamilyMember Member { get; set; }
    }

    // One-to-One: File Storage
    public class FamilyMemberFiles
    {
        [Key]
        public int FileId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [MaxLength(50)]
        public string MarriageFileNo { get; set; }

        [MaxLength(50)]
        public string BaptismFileNo { get; set; }

        [MaxLength(50)]
        public string DeathFileNo { get; set; }

        [MaxLength(50)]
        public string JoinFileNo { get; set; }

        [MaxLength(50)]
        public string MooronFileNo { get; set; }

        [MaxLength(50)]
        public string CommonCellNo { get; set; }

        [ForeignKey("MemberId")]
        public virtual FamilyMember Member { get; set; }
    }

    // One-to-One: Lifecycle Events
    public class FamilyMemberLifecycle
    {
        [Key]
        public int LifecycleId { get; set; }

        [Required]
        public int MemberId { get; set; }

        public bool CommonCell { get; set; } = false;
        public string LeftReason { get; set; }

        public DateTime? JoinDate { get; set; }
        public DateTime? LeftDate { get; set; }
        public BurialPlace? BurialPlace { get; set; }
        public DateTime? DeathDate { get; set; }

        [ForeignKey("MemberId")]
        public virtual FamilyMember Member { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public enum MaritalStatus
    {
        Single,
        Married,
        Widowed,
        Divorced
    }

    public enum MemberStatus
    {
        Alive,
        Left,
        Dead
    }

    public enum BurialPlace
    {
        CommonCell,
        Individual
    }

    public enum StudentOrEmployee
    {
        Student,
        Employee,
        Unemployed,
        SelfEmployed
    }

}

