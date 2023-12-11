using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal sealed class ParserInput(IImmutableList<IniToken> tokens) : IParserInput
{
    private static readonly IniToken.Epsilon EpsilonToken = new();

    private int _position;

    public IniToken Peek(int lookAhead = 0)
        => _position + lookAhead < tokens.Count
            ? tokens[_position + lookAhead]
            : EpsilonToken;

    public IniToken Read()
        => _position < tokens.Count
            ? tokens[_position++]
            : EpsilonToken;
}
