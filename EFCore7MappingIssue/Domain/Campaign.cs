namespace EFCore7MappingIssue.Domain;

public class Campaign
{
    public Guid Id { get; set; }
    public TriggerType TriggerType { get; set; }
}
