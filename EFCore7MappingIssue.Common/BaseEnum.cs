using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EFCore7MappingIssue.Common;

public abstract class BaseEnum<TEnum> :
    BaseEnum<TEnum, int>
    where TEnum : BaseEnum<TEnum, int>
{
    protected BaseEnum(int value, string name) :
        base(value, name)
    { }
}

public abstract class BaseEnum<TEnum, TValue> :
    IEquatable<BaseEnum<TEnum, TValue>>,
    IComparable<BaseEnum<TEnum, TValue>>
    where TEnum : BaseEnum<TEnum, TValue>
    where TValue : struct, IEquatable<TValue>, IComparable<TValue>
{
    private static readonly Lazy<List<TEnum>> _list =
        new(GetAllItems, LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<Dictionary<string, TEnum>> _fromName =
        new(() => _list.Value.ToDictionary(item => item.Name));

    private static readonly Lazy<Dictionary<string, TEnum>> _fromNameIgnoreCase =
        new(() => _list.Value.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase));

    private static readonly Lazy<Dictionary<TValue, TEnum>> _fromValue =
        new(() => _list.Value.Distinct().ToDictionary(item => item.Value));

    private static List<TEnum> GetAllItems()
    {
        var baseType = typeof(TEnum);
        return Assembly.GetAssembly(baseType)!
            .GetTypes()
            .Where(t => baseType.IsAssignableFrom(t))
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                            .Where(fi => t.IsAssignableFrom(fi.FieldType))
                            .Select(fi => Check.For.Null((TEnum?)fi.GetValue(null))))
            .ToList();
    }

    public static IReadOnlyList<TEnum> List => _list.Value;

    public TValue Value { get; }
    public string Name { get; }

    public static ValueList<string> GetAllElementNames()
    {
        var baseType = typeof(TEnum);

        var list = Assembly.GetAssembly(baseType)!
            .GetTypes()
            .Where(t => baseType.IsAssignableFrom(t))
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                            .Where(fi => t.IsAssignableFrom(fi.FieldType))
                            .Select(fi => Check.For.Null((TEnum?)fi.GetValue(null)).Name))
            .ToList();

        var vl = new ValueList<string>(list);
        return vl;
    }

    protected BaseEnum(TValue value, string name)
    {
        Value = Check.For.Null(value);
        Name = Check.For.NullOrEmpty(name);
    }

    public static TEnum FromName([AllowNull] string name, bool ignoreCase = true)
    {
        var dictionary = ignoreCase ? _fromNameIgnoreCase.Value : _fromName.Value;

        if (string.IsNullOrEmpty(name) || !dictionary.TryGetValue(name, out var result))
        {
            throw new InvalidTypeException($"Invalid name: '{name ?? "null"}', for type: '{typeof(TEnum).Name}'. Possible values are: {string.Join(", ", List.Select(x => x.Name))}");
        }
        return result;
    }

    public static bool TryFromName([AllowNull] string name, [MaybeNullWhen(false)] out TEnum result) => TryFromName(name, true, out result);

    public static bool TryFromName([AllowNull] string name, bool ignoreCase, [MaybeNullWhen(false)] out TEnum result)
    {
        if (string.IsNullOrEmpty(name))
        {
            result = default;
            return false;
        }

        return ignoreCase ? _fromNameIgnoreCase.Value.TryGetValue(name, out result) : _fromName.Value.TryGetValue(name, out result);
    }

    public static TEnum FromValue(TValue value)
    {
        if (!_fromValue.Value.TryGetValue(value, out var result))
        {
            throw new InvalidTypeException($"Invalid name: '{value}', for type: '{typeof(TEnum).Name}'. Possible values are: {string.Join(", ", List.Select(x => x.Value))}");
        }
        return result;
    }

    public static bool TryFromValue(TValue value, [MaybeNullWhen(false)] out TEnum result)
    {
        return _fromValue.Value.TryGetValue(value, out result);
    }

    /// <summary>
    /// Used for EF configurations. If the input is null returns null, otherwise falls back to FromName()
    /// </summary>
    public static TEnum? FromNameOrNull(string? name, bool ignoreCase = true)
    {
        if (name is null) return null;

        return FromName(name, ignoreCase);
    }

    /// <summary>
    /// Used for EF configurations. If the input is null returns null, otherwise falls back to FromValue()
    /// </summary>
    public static TEnum? FromValueOrNull(TValue? value)
    {
        if (value is null) return null;

        return FromValue(value.Value);
    }

    public override string ToString() => Name;

    public override int GetHashCode() => Value.GetHashCode();

    public override bool Equals(object? obj) => (obj is BaseEnum<TEnum, TValue> other) && Equals(other);

    public virtual bool Equals(BaseEnum<TEnum, TValue>? other)
    {
        if (ReferenceEquals(this, other)) return true;

        if (other is null) return false;

        return Value.Equals(other.Value);
    }

    public static bool operator ==(BaseEnum<TEnum, TValue>? left, BaseEnum<TEnum, TValue>? right)
    {
        if (left is null) return right is null;

        return left.Equals(right);
    }

    public static bool operator !=(BaseEnum<TEnum, TValue>? left, BaseEnum<TEnum, TValue>? right) => !(left == right);

    public virtual int CompareTo(BaseEnum<TEnum, TValue>? other) => other is null ? 1 : Value.CompareTo(other.Value);

    public static bool operator <(BaseEnum<TEnum, TValue>? left, BaseEnum<TEnum, TValue>? right) => left is not null && right is not null && left.CompareTo(right) < 0;

    public static bool operator <=(BaseEnum<TEnum, TValue>? left, BaseEnum<TEnum, TValue>? right) => left is not null && right is not null && left.CompareTo(right) <= 0;

    public static bool operator >(BaseEnum<TEnum, TValue>? left, BaseEnum<TEnum, TValue>? right) => left is not null && right is not null && left.CompareTo(right) > 0;

    public static bool operator >=(BaseEnum<TEnum, TValue>? left, BaseEnum<TEnum, TValue>? right) => left is not null && right is not null && left.CompareTo(right) >= 0;

    public static implicit operator TValue?(BaseEnum<TEnum, TValue>? baseEnum) => baseEnum?.Value ?? default;
    public static implicit operator string?(BaseEnum<TEnum, TValue>? baseEnum) => baseEnum?.Name ?? default;

    public static explicit operator BaseEnum<TEnum, TValue>(TValue value) => FromValue(value);
}

public static class BaseEnumExtensions
{
    public static TValue? ToValueOrNull<TEnum, TValue>(this BaseEnum<TEnum, TValue>? baseEnum)
        where TEnum : BaseEnum<TEnum, TValue>
        where TValue : struct, IEquatable<TValue>, IComparable<TValue>
    {
        if (baseEnum is null) return null;

        return baseEnum.Value;
    }

    public static string? ToNameOrNull<TEnum, TValue>(this BaseEnum<TEnum, TValue>? baseEnum)
        where TEnum : BaseEnum<TEnum, TValue>
        where TValue : struct, IEquatable<TValue>, IComparable<TValue>
    {
        if (baseEnum is null) return null;

        return baseEnum.Name;
    }
}
