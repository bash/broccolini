using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal sealed class ParserInput(IImmutableList<IniToken> tokens) : IParserInput
{
    private int _position;

    public IniToken Peek(int lookAhead = 0)
        => _position + lookAhead < tokens.Count
            ? tokens[_position + lookAhead]
            : IniToken.Epsilon.Instance;

    public IEnumerable<IniToken> PeekRange()
        => tokens.Skip(_position);

    public ImmutableArray<IniToken> Read(IEnumerable<IniToken> peeked)
    {
        var collected = peeked.ToImmutableArray();
        if (_position + collected.Length > tokens.Count) throw new InvalidOperationException($"Unable to read {collected.Length} tokens, only {tokens.Count - _position} available");
        _position += collected.Length;
        return collected;
    }

    public IniToken Read()
        => _position < tokens.Count
            ? tokens[_position++]
            : IniToken.Epsilon.Instance;
}
