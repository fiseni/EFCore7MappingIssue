using AutoMapper;
using EFCore7MappingIssue.Domain;
using EFCore7MappingIssue.Models;

namespace EFCore7MappingIssue;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TriggerType, TriggerTypeModel>();
        CreateMap<Campaign, CampaignSearchResultModel>();
    }
}
