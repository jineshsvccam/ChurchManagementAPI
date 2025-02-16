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

        }
    }
}
