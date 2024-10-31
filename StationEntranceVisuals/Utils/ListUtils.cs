using System;
using System.Collections.Generic;

namespace StationEntranceVisuals.Utils;

public static class EnumerableExtensions
{
    public static T ElementAtOrValue<T>(
        this IEnumerable<T> source, int index, T defaultValue)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (index < 0) return defaultValue;
        if (source is IList<T> list)
        {
            if (index < list.Count)
            {
                return list[index];
            }
            else
            {
                return defaultValue;
            };
        }
        else
        {
            using var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (index-- == 0) return enumerator.Current;
            }
        }
        return defaultValue;
    }
}