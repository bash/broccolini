using FsCheck;
using FsCheck.Fluent;

namespace Broccolini.Test;

internal static class BroccoliniArbMap
{
    private static readonly char[] WhitespaceChars =
        Enumerable.Range(0, count: ' ' + 1)
            .Select(n => (char)n)
            .Except([ '\r', '\n' ])
            .ToArray();

    private static readonly char[] NewLineChars = ['\r', '\n'];

    public static IArbMap Default { get; } =
        ArbMap.Default
            .MergeArbFactory(ArbitrarySectionName)
            .MergeArbFactory(ArbitrarySectionNameNoNulls)
            .MergeArb(ArbitraryWhitespaceChar())
            .MergeArbFactory(ArbitraryWhitespace)
            .MergeArbFactory(ArbitraryWhitespaceNoNulls)
            .MergeArbFactory(ArbitraryInlineText)
            .MergeArbFactory(ArbitraryInlineTextNoNulls);

    private static Arbitrary<SectionName> ArbitrarySectionName(IArbMap arbMap)
        => arbMap.ArbFor<string>()
            .Filter(x => x is not null)
            .Filter(x => !x.Contains(']') && !x.ContainsAny(NewLineChars))
            .Filter(x => x.Trim(WhitespaceChars) == x)
            .Convert(x => new SectionName(x), x => x.Value);

    private static Arbitrary<SectionNameNoNulls> ArbitrarySectionNameNoNulls(IArbMap arbMap)
        => arbMap.ArbFor<SectionName>()
            .Filter(x => !x.Value.Contains('\0'))
            .Convert(s => new SectionNameNoNulls(s.Value), s => new SectionName(s.Value));

    public static Arbitrary<WhitespaceNoNulls> ArbitraryWhitespaceNoNulls(IArbMap arbMap)
        => arbMap.ArbFor<Whitespace>()
            .Filter(w => !w.Value.Contains('\0'))
            .Convert(w => new WhitespaceNoNulls(w.Value), w => new Whitespace(w.Value));

    private static Arbitrary<Whitespace> ArbitraryWhitespace(IArbMap arbMap)
        => arbMap.ArbFor<List<WhitespaceChar>>()
            .Convert(l => new Whitespace(l.Select(c => c.Value).ConcatToString()), w => w.Value.Select(c => new WhitespaceChar(c)).ToList());

    private static Arbitrary<WhitespaceChar> ArbitraryWhitespaceChar()
        => Arb.From(Gen.OneOf(WhitespaceChars.Select(Gen.Constant)).Select(c => new WhitespaceChar(c)));

    private static Arbitrary<InlineText> ArbitraryInlineText(IArbMap arbMap)
        => arbMap.ArbFor<string>()
            .Filter(x => x is not null)
            .Filter(x => !x.ContainsAny(NewLineChars))
            .Convert(s => new InlineText(s), t => t.Value);

    private static Arbitrary<InlineTextNoNulls> ArbitraryInlineTextNoNulls(IArbMap arbMap)
        => arbMap.ArbFor<InlineText>()
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
