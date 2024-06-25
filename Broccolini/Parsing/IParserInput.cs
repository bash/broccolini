using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal interface IParserInput
{
    int AvailableLength { get; }

    IniToken Peek(int lookAhead = 0);

    IEnumerable<IniToken> PeekRange();

    IniToken Read();

    ImmutableArray<IniToken> Read(IEnumerable<IniToken> peeked);
}
