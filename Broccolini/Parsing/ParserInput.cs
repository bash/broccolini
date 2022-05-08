using System.Collections.Immutable;
using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal sealed class ParserInput : IParserInput
{
    private static readonly Token.Epsilon EpsilonToken = new();

    private readonly IImmutableList<Token> _tokens;
    private int _position;

    public ParserInput(IImmutableList<Token> tokens) => _tokens = tokens;

    public Token Peek(int lookAhead = 0)
        => _position + lookAhead < _tokens.Count
            ? _tokens[_position + lookAhead]
            : EpsilonToken;

    public Token Read()
        => _position < _tokens.Count
            ? _tokens[_position++]
            : EpsilonToken;
}
