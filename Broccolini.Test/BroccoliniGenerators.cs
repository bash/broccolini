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

    public static Arbitrary<Whitespace> ArbitraryWhitespace()
        => Arb.From<List<WhitespaceChar>>()
            .Convert(l => new Whitespace(l.Select(c => c.Value).ConcatToString()), w => w.Value.Select(c => new WhitespaceChar(c)).ToList());

    public static Arbitrary<WhitespaceNoNulls> ArbitraryWhitespaceNoNulls()
        => Arb.From<Whitespace>()
            .Filter(w => !w.Value.Contains('\0'))
            .Convert(w => new WhitespaceNoNulls(w.Value), w => new Whitespace(w.Value));

    public static Arbitrary<WhitespaceChar> ArbitraryWhitespaceChar()
        => Arb.From(Gen.OneOf(WhitespaceChars.Select(Gen.Constant)).Select(c => new WhitespaceChar(c)));

    public static Arbitrary<InlineText> ArbitraryInlineText()
        => Arb.From<string>()
            .Filter(x => x is not null)
            .Filter(x => !x.ContainsAny(NewLineChars))
            .Convert(s => new InlineText(s), t => t.Value);

    public static Arbitrary<InlineTextNoNulls> ArbitraryInlineTextNoNulls()
        => Arb.From<InlineText>()
            .Filter(w => !w.Value.Contains('\0'))
            .Convert(t => new InlineTextNoNulls(t.Value), t => new InlineText(t.Value));

    private static bool ContainsAny(this string input, char[] chars)
        => input.IndexOfAny(chars) != -1;
}

public sealed record SectionName(string Value);

public sealed record SectionNameNoNulls(string Value);

public sealed record Whitespace(string Value);

public sealed record WhitespaceNoNulls(string Value);

public sealed record WhitespaceChar(char Value);

public sealed record InlineText(string Value);

public sealed record InlineTextNoNulls(string Value);
