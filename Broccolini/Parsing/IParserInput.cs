using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal interface IParserInput
{
    Token Peek(int lookAhead = 0);

    Token Read();
}
