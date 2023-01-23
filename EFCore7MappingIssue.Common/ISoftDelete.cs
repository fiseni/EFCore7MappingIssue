namespace EFCore7MappingIssue.Common;

public interface ISoftDelete
{
    public bool IsDeleted { get; }
}
