using AutoMapper;
using ChurchDTOs.DTOs.Entities;




namespace ChurchData.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Bank, BankDto>().ReverseMap();
            CreateMap<ContributionSettings, ContributionSettingsDto>().ReverseMap();
            CreateMap<Diocese, DioceseDto>().ReverseMap();
            CreateMap<District, DistrictDto>().ReverseMap();
            CreateMap<Family, FamilyDto>().ReverseMap();
            CreateMap<FamilyContribution, FamilyContributionDto>().ReverseMap();
            CreateMap<FamilyDue, FamilyDueDto>().ReverseMap();
            //family member 
            CreateMap<FinancialYear, FinancialYearDto>().ReverseMap();
            CreateMap<Parish, ParishDetailsDto>().ReverseMap();
            CreateMap<Parish, ParishDto>().ReverseMap();
            //pending family member action           
            CreateMap<RecurringTransaction, RecurringTransactionDto>().ReverseMap();
            CreateMap<Transaction, TransactionDto>().ReverseMap();
            CreateMap<TransactionHead, TransactionHeadDto>().ReverseMap();
            CreateMap<Unit, UnitDto>().ReverseMap();






        }
    }
}
