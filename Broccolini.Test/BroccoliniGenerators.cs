using FsCheck;

namespace Broccolini.Test;

internal static class BroccoliniGenerators
{
    private static readonly char[] WhitespaceChars =
        Enumerable.Range(0, count: ' ' + 1)
            .Select(n => (char)n)
            .Except(new[] { '\r', '\n' })
            .ToArray();

    private static readonly char[] NewLineChars = { '\r', '\n' };

    public static void Register()
    {
        Arb.Register(typeof(BroccoliniGenerators));
    }

    public static Arbitrary<SectionName> ArbitrarySectionName()
        => Arb.From<string>()
            .Filter(x => x is not null)
            .Filter(x => !x.Contains(']') && !x.ContainsAny(NewLineChars))
            .Filter(x => x.Trim(WhitespaceChars) == x)
            .Convert(x => new SectionName(x), x => x.Value);

    public static Arbitrary<SectionNameNoNulls> ArbitrarySectionNameNoNulls()
        => ArbitrarySectionName()
            .Filter(x => !x.Value.Contains('\0'))
            .Convert(s => new SectionNameNoNulls(s.Value), s => new SectionName(s.Value));

    private static bool ContainsAny(this string input, char[] chars)
        => input.IndexOfAny(chars) != -1;
}

public sealed record SectionName(string Value);

public sealed record SectionNameNoNulls(string Value);
