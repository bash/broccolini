using Broccolini.Syntax;
using static Broccolini.Tokenization.Rules.CharPredicates;

namespace Broccolini.Tokenization.Rules;

internal sealed class LineBreakRule : ITokenizerRule
{
    public Token? Match(ITokenizerInput input)
    {
        if (input.Peek(out var firstCharacter) && firstCharacter == '\r' && input.Peek(out var secondCharacter, 1) && secondCharacter == '\n')
        {
            input.Read();
            input.Read();
            return new Token.LineBreak("\r\n");
        }

        if (input.Peek(out var character) && IsLineBreak(character))
        {
            return new Token.LineBreak(input.Read().ToString());
        }

        return null;
    }
}
