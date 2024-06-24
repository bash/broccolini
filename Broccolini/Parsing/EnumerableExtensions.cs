namespace Broccolini.Parsing;

internal static class EnumerableExtensions
{
    public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, TSource @default)
        => source.Append(@default).First();

    public static IEnumerable<TSource> DropLast<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => source.WithLast().Where(x => !x.IsLast || !predicate(x.Item)).Select(x => x.Item);

    private static IEnumerable<(TSource Item, bool IsLast)> WithLast<TSource>(this IEnumerable<TSource> source)
    {
        using var enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            yield break;
        }

        var current = enumerator.Current;
        while (enumerator.MoveNext())
        {
            yield return (current, false);
            current = enumerator.Current;
        }

        yield return (current, true);
    }
}
