using AutoMapper;
using ChurchDTOs.DTOs.Entities;




namespace ChurchData.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ContributionSettings, ContributionSettingsDto>().ReverseMap();
            CreateMap<FamilyDue, FamilyDueDto>().ReverseMap();
            CreateMap<FamilyContribution, FamilyContributionDto>().ReverseMap();
            CreateMap<RecurringTransaction, RecurringTransactionDto>().ReverseMap();

            CreateMap<Diocese, DioceseDto>().ReverseMap();
            CreateMap<District, DistrictDto>().ReverseMap();
            CreateMap<Parish, ParishDetailsDto>().ReverseMap();
            CreateMap<Parish, ParishDto>().ReverseMap();

        }
    }
}
