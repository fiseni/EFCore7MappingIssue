namespace EFCore7MappingIssue.Common;

internal static class TypeUtil
{
    public static bool IsDerived(Type objectType, Type mainType)
    {
        var currentType = objectType.BaseType;

        if (currentType is null)
        {
            return false;
        }

        while (currentType is not null && currentType != typeof(object))
        {
            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == mainType)
                return true;

            currentType = currentType.BaseType;
        }

        return false;
    }

    public static Type? GetBaseEnumType(Type objectType, Type mainType)
    {
        var currentType = objectType.BaseType;

        if (currentType == null)
        {
            return null;
        }

        while (currentType is not null && currentType != typeof(object))
        {
            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == mainType)
                return currentType;

            currentType = currentType.BaseType;
        }

        return null;
    }
}
