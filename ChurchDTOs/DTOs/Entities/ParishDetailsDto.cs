using ChurchDTOs.DTOs.Utils;


namespace ChurchDTOs.DTOs.Entities
{
    public class ParishDetailsDto : IParishEntity
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
        public ICollection<FinancialYearDto>? FinancialYears { get; set; }
    }

    public class ParishDto : IParishEntity
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
     
    }

    public class ParishSimpleDto
    {
        public int ParishId { get; set; }
        public required string ParishName { get; set; }
        public string? ParishLocation { get; set; }       
        public int DistrictId { get; set; }
        public List<UnitSimpleDto> Units { get; set; }
    }

    public class ParishDetailsBasicDto : IParishEntity
    {
        public int ParishId { get; set; }
        public required string ParishName { get; set; }      

        public ICollection<UnitBasicDto>? Units { get; set; }
        public ICollection<FamilyBasicDto>? Families { get; set; }
        public ICollection<TransactionHeadBasicDto>? TransactionHeads { get; set; }
        public ICollection<BankBasicDto>? Banks { get; set; }
        public ICollection<TransactionDto>? Transactions { get; set; }
        public ICollection<FamilyMemberDto>? FamilyMembers { get; set; }
        public ICollection<FinancialYearBasicDto>? FinancialYears { get; set; }
    }


}
