using Broccolini.Syntax;
using static Broccolini.Tokenization.Rules.CharPredicates;

namespace Broccolini.Tokenization.Rules;

internal sealed class NewLineRule : ITokenizerRule
{
    public IniToken? Match(ITokenizerInput input)
    {
        if (input.Peek(out var firstCharacter) && firstCharacter == '\r' && input.Peek(out var secondCharacter, 1) && secondCharacter == '\n')
        {
            input.Read();
            input.Read();
            return new IniToken.NewLine("\r\n");
        }

        if (input.Peek(out var character) && IsNewLine(character))
        {
            return new IniToken.NewLine(input.Read().ToString());
        }

        return null;
    }
}
