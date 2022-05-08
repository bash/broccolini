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

    public static IImmutableList<Token> ReadOrEmpty(this IParserInput input, Func<Token, bool> predicate)
        => input.ReadOrNone(predicate).Match(none: ImmutableArray<Token>.Empty, some: ImmutableArray.Create);

    public static Option<Token> ReadOrNone(this IParserInput input, Func<Token, bool> predicate)
        => Option.Return(input.Peek())
            .Where(predicate)
            .Select(_ => input.Read());

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
