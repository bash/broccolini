using Broccolini.Syntax;

namespace Broccolini.Tokenization.Rules;

internal sealed class IdentifierRule : ITokenizerRule
{
    public IniToken? Match(ITokenizerInput input)
    {
        var value = input.ReadWhile(CharPredicates.IsIdentifier);
        return value.Length > 0
            ? new IniToken.Identifier(value)
            : null;
    }
}
