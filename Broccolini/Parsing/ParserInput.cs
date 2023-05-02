using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal sealed class ParserInput : IParserInput
{
    private static readonly IniToken.Epsilon EpsilonToken = new();

    private readonly IImmutableList<IniToken> _tokens;
    private int _position;

    public ParserInput(IImmutableList<IniToken> tokens) => _tokens = tokens;

    public IniToken Peek(int lookAhead = 0)
        => _position + lookAhead < _tokens.Count
            ? _tokens[_position + lookAhead]
            : EpsilonToken;

    public IniToken Read()
        => _position < _tokens.Count
            ? _tokens[_position++]
            : EpsilonToken;
}
