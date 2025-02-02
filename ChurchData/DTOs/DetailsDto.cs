using System;
using System.Collections.Generic;

namespace ChurchData.DTOs
{
    // ParishDetailsDto class
    public class ParishDetailsDto
    {
        public int ParishId { get; set; }
        public required string ParishName { get; set; } 
        public string? ParishLocation { get; set; } 
        public string? Photo { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Place { get; set; }
        public string? Pincode { get; set; }
        public string? VicarName { get; set; }
        public int DistrictId { get; set; }

        public ICollection<UnitDto>? Units { get; set; }
        public ICollection<FamilyDto>? Families { get; set; }
        public ICollection<TransactionHeadDto>? TransactionHeads { get; set; }
        public ICollection<BankDto>? Banks { get; set; }
        public ICollection<TransactionDto>? Transactions { get; set; }
        public ICollection<FamilyMemberDto>? FamilyMembers { get; set; }
    }

    // UnitDto class
    public class UnitDto
    {
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? Description { get; set; }
        public string? UnitPresident { get; set; }
        public string? UnitSecretary { get; set; }
    }

    // FamilyDto class
    public class FamilyDto
    {
        public int FamilyId { get; set; }
        public string? FamilyName { get; set; }
        public string? Address { get; set; }
        public string? ContactInfo { get; set; }
        public string? Category { get; set; }
        public int FamilyNumber { get; set; }
        public string? Status { get; set; }
        public string? HeadName { get; set; }
        public DateTime JoinDate { get; set; }
    }

    // TransactionHeadDto class
    public class TransactionHeadDto
    {
        public int HeadId { get; set; }
        public string HeadName { get; set; } = null!;
        public string? Type { get; set; }
        public string? Description { get; set; }
    }

    // BankDto class
    public class BankDto
    {
        public int BankId { get; set; }
        public string BankName { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
    }
    // TransactionDto class
    public class TransactionDto
    {
        public int TransactionId { get; set; }
        public DateTime TrDate { get; set; }
        public string? VrNo { get; set; } 
        public string? TransactionType { get; set; } 
        public decimal IncomeAmount { get; set; }
        public decimal ExpenseAmount { get; set; }
        public string? Description { get; set; }
        public int? HeadId { get; set; }
        public int? FamilyId { get; set; }
        public int? BankId { get; set; }

        //public string? HeadName { get; set; }
        //public string? BankName { get; set; }
        //public string? FamilyName { get; set; }
    }
    // FamilyMemberDto class
    public class FamilyMemberDto
    {
        public int FamilyId { get; set; }
        public int UnitId { get; set; }
        public int MemberId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = null!;
        public string? ContactInfo { get; set; }
        public string Role { get; set; } = null!;

    }
}
