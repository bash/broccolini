using Broccolini.Syntax;

namespace Broccolini.Tokenization.Rules;

internal sealed class WhiteSpaceRule : ITokenizerRule
{
    public Token? Match(ITokenizerInput input, IReadOnlyList<Token> context)
    {
        var value = input.ReadWhile(CharPredicates.IsWhitespace);
        return value.Length > 0
            ? new Token.WhiteSpace(value)
            : null;
    }
}
