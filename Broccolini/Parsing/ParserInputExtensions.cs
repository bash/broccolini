using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal static class ParserInputExtensions
{
    public static IniToken PeekIgnoreWhitespace(this IParserInput input, int lookAhead = 0)
    {
        var current = input.Peek(lookAhead);
        return current is IniToken.WhiteSpace
            ? input.Peek(lookAhead + 1)
            : current;
    }

    public static IniToken PeekIgnoreWhitespacesNewLines(this IParserInput input)
    {
        var lookAhead = 0;
        for (; input.Peek(lookAhead) is IniToken.WhiteSpace or IniToken.NewLine; lookAhead++) { }

        return input.Peek(lookAhead);
    }

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
        => input.ReadWhile(static input => input.Peek(), (t, _) => predicate(t));

    public static IImmutableList<IniToken> ReadWhile(this IParserInput input, Func<IniToken, IParserInput, bool> predicate)
        => input.ReadWhile(static input => input.Peek(), predicate);

    public static IImmutableList<IniToken> ReadWhileExcludeTrailingWhitespace(this IParserInput input, Func<IniToken, bool> predicate)
        => input.ReadWhile(static input => input.PeekIgnoreWhitespace(), (t, _) => predicate(t));

    private static IImmutableList<IniToken> ReadWhile(this IParserInput input, Func<IParserInput, IniToken> peek, Func<IniToken, IParserInput, bool> predicate)
    {
        var tokens = ImmutableArray.CreateBuilder<IniToken>();

        while (true)
        {
            var token = peek(input);
            if (token is IniToken.Epsilon || !predicate(token, input)) break;
            tokens.Add(input.Read());
        }

        return tokens.ToImmutable();
    }
}
