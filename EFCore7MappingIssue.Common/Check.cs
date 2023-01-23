namespace EFCore7MappingIssue.Common;

public class Check : ICheckClause
{
    private Check() { }

    public static ICheckClause For { get; } = new Check();
}
