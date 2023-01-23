namespace EFCore7MappingIssue.Models;

public record CampaignSearchResultModel
{
    public Guid Id { get; set; }
    public TriggerTypeModel TriggerType { get; set; }
}
