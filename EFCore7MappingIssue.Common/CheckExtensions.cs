using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EFCore7MappingIssue.Common;

public static class CheckExtensions
{
    public static T NullWithMessage<T>(this ICheckClause checkClause, [NotNull] T? input, string message)
    {
        if (input is null)
        {
            throw new ArgumentNullException(message, (Exception?)null);
        }

        return input;
    }

    public static T Null<T>(this ICheckClause checkClause, [NotNull] T? input, [CallerArgumentExpression("input")] string? parameterName = null)
    {
        return checkClause.NullWithMessage(input, $"Required input {parameterName} was null.");
    }

    public static string NullOrEmpty(this ICheckClause checkClause, [NotNull] string? input, [CallerArgumentExpression("input")] string? parameterName = null)
    {
        checkClause.Null(input, parameterName);

        if (input == string.Empty)
        {
            throw new ArgumentException($"Required input {parameterName} was empty.", parameterName);
        }

        return input;
    }
}
