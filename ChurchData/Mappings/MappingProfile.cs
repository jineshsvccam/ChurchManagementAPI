using AutoMapper;
using ChurchDTOs.DTOs.Entities;
using ChurchDTOs.DTOs.Utils;




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

            CreateMap<FinancialReportsView, FinancialReportCustomDTO>()
            .ForMember(dest => dest.HeadId, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
                return (option == FinancialReportCustomizationOption.IdsOnly || option == FinancialReportCustomizationOption.Both) ? src.HeadId : (int?)null;
            }))
            .ForMember(dest => dest.HeadName, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
                return (option == FinancialReportCustomizationOption.NamesOnly || option == FinancialReportCustomizationOption.Both) ? src.HeadName : null;
            }))
            .ForMember(dest => dest.FamilyId, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
                return (option == FinancialReportCustomizationOption.IdsOnly || option == FinancialReportCustomizationOption.Both) ? src.FamilyId : (int?)null;
            }))
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
                return (option == FinancialReportCustomizationOption.NamesOnly || option == FinancialReportCustomizationOption.Both) ? src.FamilyName : null;
            }))
            .ForMember(dest => dest.BankId, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
                return (option == FinancialReportCustomizationOption.IdsOnly || option == FinancialReportCustomizationOption.Both) ? src.BankId : (int?)null;
            }))
            .ForMember(dest => dest.BankName, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
                return (option == FinancialReportCustomizationOption.NamesOnly || option == FinancialReportCustomizationOption.Both) ? src.BankName : null;
            }))
            .ForMember(dest => dest.ParishId, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
                return (option == FinancialReportCustomizationOption.IdsOnly || option == FinancialReportCustomizationOption.Both) ? src.ParishId : (int?)null;
            }))
            .ForMember(dest => dest.ParishName, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
                return (option == FinancialReportCustomizationOption.NamesOnly || option == FinancialReportCustomizationOption.Both) ? src.ParishName : null;
            }));

            //for notice board
            CreateMap<FinancialReportsView, FinancialReportNoticeBoardDTO>()
            .ForMember(dest => dest.FamilyId, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
                return (option == FinancialReportCustomizationOption.IdsOnly || option == FinancialReportCustomizationOption.Both)
                        ? src.FamilyId
                        : (int?)null;
            }))
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
                // The view is assumed to already combine head name and family name in FamilyName.
                return (option == FinancialReportCustomizationOption.NamesOnly || option == FinancialReportCustomizationOption.Both)
                        ? src.FamilyName
                        : null;
            }))
            .ForMember(dest => dest.UnitId, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
                return (option == FinancialReportCustomizationOption.IdsOnly || option == FinancialReportCustomizationOption.Both)
                        ? src.UnitId
                        : (int?)null;
            }))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
                return (option == FinancialReportCustomizationOption.NamesOnly || option == FinancialReportCustomizationOption.Both)
                        ? src.UnitName
                        : null;
            }))
            .ForMember(dest => dest.FamilyNumber, opt => opt.MapFrom(src => src.FamilyNumber))
            .ForMember(dest => dest.TrDate, opt => opt.MapFrom(src => src.TrDate))
            .ForMember(dest => dest.VrNo, opt => opt.MapFrom(src => src.VrNo))
            .ForMember(dest => dest.IncomeAmount, opt => opt.MapFrom(src => src.IncomeAmount));



        }
    }
}
