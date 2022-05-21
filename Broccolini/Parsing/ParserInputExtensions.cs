using System.Collections.Immutable;
using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal static class ParserInputExtensions
{
    public static Token PeekIgnoreWhitespace(this IParserInput input, int lookAhead = 0)
    {
        var current = input.Peek(lookAhead);
        return current is Token.WhiteSpace
            ? input.Peek(lookAhead + 1)
            : current;
    }

    public static TToken? ReadOrNull<TToken>(this IParserInput input) where TToken : Token => ReadOrNull<TToken>(input, static _ => true);

    public static TToken? ReadOrNull<TToken>(this IParserInput input, Func<TToken, bool> predicate)
        where TToken : Token
    {
        var token = input.Peek();

        if (token is TToken t && predicate(t))
        {
            input.Read();
            return t;
        }

        return null;
    }

    public static IImmutableList<Token> ReadWhile(this IParserInput input, Func<Token, bool> predicate)
        => input.ReadWhile(static input => input.Peek(), predicate);

    public static IImmutableList<Token> ReadWhileExcludeTrailingWhitespace(this IParserInput input, Func<Token, bool> predicate)
        => input.ReadWhile(static input => input.PeekIgnoreWhitespace(), predicate);

    private static IImmutableList<Token> ReadWhile(this IParserInput input, Func<IParserInput, Token> peek, Func<Token, bool> predicate)
    {
        var tokens = ImmutableArray.CreateBuilder<Token>();

        while (true)
        {
            var token = peek(input);
            if (token is Token.Epsilon || !predicate(token)) break;
            tokens.Add(input.Read());
        }

        return tokens.ToImmutable();
    }
}
