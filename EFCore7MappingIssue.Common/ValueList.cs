namespace EFCore7MappingIssue.Common;


public class ValueList<T> : List<T>
{
    public ValueList(IEnumerable<T> collection) : base(collection)
    {
    }

    public ValueList() : base()
    {
    }

    public ValueList(int capacity) : base(capacity)
    {
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not ValueList<T> list)
        {
            return false;
        }
        else if (list.Count != Count)
        {
            return false;
        }
        else
        {
            for (var i = 0; i < Count; i++)
            {
                if (!list.Contains(this[0])) return false;
            }
        }

        return true;
    }


    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        foreach (var item in this)
        {
            hashCode.Add(item);
        }

        return hashCode.ToHashCode();
    }
}

public static class ValueListExtensions
{
    public static ValueList<T> ToValueList<T>(this IEnumerable<T> collection)
    {
        return new ValueList<T>(collection);
    }
}
