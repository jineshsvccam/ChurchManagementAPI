using AutoMapper;
using ChurchData.Entities;
using ChurchDTOs.DTOs.Entities;
using ChurchDTOs.DTOs.Utils;
using NetTopologySuite.Geometries;




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
          
            CreateMap<FamilyContribution, FamilyContributionDto>().ReverseMap();
            CreateMap<FamilyDue, FamilyDueDto>().ReverseMap();
            //family member 
            CreateMap<FinancialYear, FinancialYearDto>().ReverseMap();

            //pending family member action           
            CreateMap<RecurringTransaction, RecurringTransactionDto>().ReverseMap();
            CreateMap<Transaction, TransactionDto>().ReverseMap();
            CreateMap<TransactionHead, TransactionHeadDto>().ReverseMap();
            CreateMap<Unit, UnitDto>().ReverseMap();
            CreateMap<Role, RoleDto>().ReverseMap();

            CreateMap<UserRole, PendingUserRoleDto>()
           .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name))
           .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
           .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
           .ForMember(dest => dest.ParishId, opt => opt.MapFrom(src => src.User.ParishId)) // Keep ParishId mapping
           .ForMember(dest => dest.FamilyId, opt => opt.MapFrom(src => src.User.FamilyId)) // Use correct field name
           .ForMember(dest => dest.ParishName, opt => opt.MapFrom(src => src.User.Parish != null ? src.User.Parish.ParishName : ""))
           .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src =>
               src.User.Family != null ?
               string.Concat(src.User.Family.HeadName ?? "", " ", src.User.Family.FamilyName ?? "").Trim()
               : ""))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));


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
            }));
            //.ForMember(dest => dest.ParishId, opt => opt.MapFrom((src, dest, destMember, context) =>
            //{
            //    var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
            //    return (option == FinancialReportCustomizationOption.IdsOnly || option == FinancialReportCustomizationOption.Both) ? src.ParishId : (int?)null;
            //}));
            //.ForMember(dest => dest.ParishName, opt => opt.MapFrom((src, dest, destMember, context) =>
            //{
            //    var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
            //    return (option == FinancialReportCustomizationOption.NamesOnly || option == FinancialReportCustomizationOption.Both) ? src.ParishName : null;
            //}));

            //dues table
            CreateMap<FinancialReportsViewDues, FinancialReportCustomDTO>()
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
         }));
            //.ForMember(dest => dest.ParishId, opt => opt.MapFrom((src, dest, destMember, context) =>
            //{
            //    var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
            //    return (option == FinancialReportCustomizationOption.IdsOnly || option == FinancialReportCustomizationOption.Both) ? src.ParishId : (int?)null;
            //}));
            //.ForMember(dest => dest.ParishName, opt => opt.MapFrom((src, dest, destMember, context) =>
            //{
            //    var option = (FinancialReportCustomizationOption)context.Items["CustomizationOption"];
            //    return (option == FinancialReportCustomizationOption.NamesOnly || option == FinancialReportCustomizationOption.Both) ? src.ParishName : null;
            //}));


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

            CreateMap<FamilyFile, FamilyFileDto>();
            CreateMap<FamilyFileCreateDto, FamilyFile>();

            // Parish → DTO (Response)
            CreateMap<Parish, ParishDto>()
                .ForMember(dest => dest.Latitude,
                    opt => opt.MapFrom(src => src.GeoLocation != null ? (decimal?)src.GeoLocation.Y : null))
                .ForMember(dest => dest.Longitude,
                    opt => opt.MapFrom(src => src.GeoLocation != null ? (decimal?)src.GeoLocation.X : null));

            // DTO → Parish (Create / Update)
            CreateMap<ParishDto, Parish>()
                .ForMember(dest => dest.GeoLocation,
                    opt => opt.MapFrom(src =>
                        src.Latitude.HasValue && src.Longitude.HasValue
                            ? new Point((double)src.Longitude.Value, (double)src.Latitude.Value) { SRID = 4326 }
                            : null
                    ));

            CreateMap<Parish, ParishDetailsDto>()
                .ForMember(dest => dest.Latitude,
                    opt => opt.MapFrom(src => src.GeoLocation != null ? (decimal?)src.GeoLocation.Y : null))
                .ForMember(dest => dest.Longitude,
                    opt => opt.MapFrom(src => src.GeoLocation != null ? (decimal?)src.GeoLocation.X : null));

            CreateMap<ParishDetailsDto, Parish>()
                .ForMember(dest => dest.GeoLocation,
                    opt => opt.MapFrom(src =>
                        src.Latitude.HasValue && src.Longitude.HasValue
                            ? new Point((double)src.Longitude.Value, (double)src.Latitude.Value) { SRID = 4326 }
                            : null
                    ));

            // Family → DTO
            CreateMap<Family, FamilyDto>()
                .ForMember(dest => dest.Latitude,
                    opt => opt.MapFrom(src => src.GeoLocation != null ? (decimal?)src.GeoLocation.Y : null))
                .ForMember(dest => dest.Longitude,
                    opt => opt.MapFrom(src => src.GeoLocation != null ? (decimal?)src.GeoLocation.X : null));

            // DTO → Family
            CreateMap<FamilyDto, Family>()
                .ForMember(dest => dest.GeoLocation,
                    opt => opt.MapFrom(src =>
                        src.Latitude.HasValue && src.Longitude.HasValue
                            ? new Point((double)src.Longitude.Value, (double)src.Latitude.Value) { SRID = 4326 }
                            : null
                    ));


        }
    }
}
