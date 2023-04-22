using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal interface IParserInput
{
    IniToken Peek(int lookAhead = 0);

    IniToken Read();
}
