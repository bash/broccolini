#if NETSTANDARD2_0
// Source: https://github.com/dotnet/runtime/blob/v5.0.18/src/libraries/System.Linq/src/System/Linq/Skip.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Linq;

internal static partial class EnumerableCompatibility
{
    public static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count)
    {
        var queue = new Queue<TSource>();
        using IEnumerator<TSource> e = source.GetEnumerator();
        while (e.MoveNext())
        {
            if (queue.Count == count)
            {
                do
                {
                    yield return queue.Dequeue();
                    queue.Enqueue(e.Current);
                }
                while (e.MoveNext());
                break;
            }
            else
            {
                queue.Enqueue(e.Current);
            }
        }
    }
}

#endif
