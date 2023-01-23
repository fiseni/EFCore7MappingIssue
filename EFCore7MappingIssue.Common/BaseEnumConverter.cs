using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace EFCore7MappingIssue.Common;

internal class BaseEnumConverter<TEnum, TEnumBase, TValue> : ValueConverter<TEnum, TValue>
    where TEnum : BaseEnum<TEnumBase, TValue>
    where TEnumBase : BaseEnum<TEnumBase, TValue>
    where TValue : struct, IEquatable<TValue>, IComparable<TValue>
{
    public BaseEnumConverter() : base(_serialize, _deserialize, null)
    {
    }

    private static readonly Expression<Func<TValue, TEnum>> _deserialize = x => (BaseEnum<TEnumBase, TValue>.FromValue(x) as TEnum)!;
    private static readonly Expression<Func<TEnum, TValue>> _serialize = x => x.Value;
}
