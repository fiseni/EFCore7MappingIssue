namespace EFCore7MappingIssue.Models;

public record TriggerTypeModel
{
    public int Value { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Position { get; set; }
    public bool HasRunTimeDefined { get; set; }
}

