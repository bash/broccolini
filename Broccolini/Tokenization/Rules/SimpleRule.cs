using Broccolini.Syntax;

namespace Broccolini.Tokenization.Rules;

internal sealed class SimpleRule(char expectedCharacter, IniToken token) : ITokenizerRule
{
    public IniToken? Match(ITokenizerInput input)
    {
        if (input.Peek(out var character) && character == expectedCharacter)
        {
            input.Read();
            return token;
        }

        return null;
    }
}
