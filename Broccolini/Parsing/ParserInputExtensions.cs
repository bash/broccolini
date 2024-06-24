using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal static class ParserInputExtensions
{
    public static IniToken PeekIgnoreWhitespace(this IParserInput input, int lookAhead = 0)
        => input.PeekRange()
            .Skip(lookAhead)
            .SkipWhile(static t => t is IniToken.WhiteSpace)
            .FirstOrEpsilon();

    public static (IniToken, int) PeekIgnoreWhitespaceAndNewLines(this IParserInput input)
        => input.PeekRange()
            .Select((t, i) => (t, i))
            .SkipWhile(p => p.t is IniToken.WhiteSpace or IniToken.NewLine)
            .FirstOrDefault((IniToken.Epsilon.Instance, 0));

    public static TToken? ReadOrNull<TToken>(this IParserInput input) where TToken : IniToken => ReadOrNull<TToken>(input, static _ => true);

    public static TToken? ReadOrNull<TToken>(this IParserInput input, Func<TToken, bool> predicate)
        where TToken : IniToken
    {
        var token = input.Peek();

        if (token is TToken t && predicate(t))
        {
            input.Read();
            return t;
        }

        return null;
    }

    public static IImmutableList<IniToken> ReadWhile(this IParserInput input, Func<IniToken, bool> predicate)
        => input.Read(input.PeekRange().TakeWhile(predicate));

    public static ImmutableArray<IniToken> ReadWhileExcludeTrailingWhitespace(this IParserInput input, Func<IniToken, bool> predicate)
        => input.Read(input.PeekRange().TakeWhile(predicate).DropLast(static t => t is IniToken.WhiteSpace));
}
