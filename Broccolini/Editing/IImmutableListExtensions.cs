using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Broccolini.Editing;

internal static class IImmutableListExtensions
{
    public static bool TryUpdateFirst<T>(this IImmutableList<T> list, Func<T, bool> predicate, Func<T, T> transformation, [NotNullWhen(true)] out IImmutableList<T>? updated)
    {
        updated = null;

        if (list.TryFindIndex(predicate, out var index))
        {
            updated = list.SetItem(index, transformation(list[index]));
            return true;
        }

        return false;
    }

    public static bool TryFindIndex<T>(this IImmutableList<T> list, Func<T, bool> predicate, out int index)
    {
        index = 0;

        foreach (var item in list)
        {
            if (predicate(item))
            {
                return true;
            }

            index++;
        }

        return false;
    }
}
