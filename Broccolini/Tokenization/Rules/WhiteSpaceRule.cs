using Broccolini.Syntax;

namespace Broccolini.Tokenization.Rules;

internal sealed class WhiteSpaceRule : ITokenizerRule
{
    public IniToken? Match(ITokenizerInput input)
    {
        var value = input.ReadWhile(CharPredicates.IsWhitespace);
        return value.Length > 0
            ? new IniToken.WhiteSpace(value)
            : null;
    }
}
