using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal interface IParserInput
{
    IniToken Peek();

    IEnumerable<IniToken> PeekRange();

    IniToken Read();

    ImmutableArray<IniToken> Read(IEnumerable<IniToken> peeked);
}
