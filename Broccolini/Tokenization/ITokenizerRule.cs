using Broccolini.Syntax;

namespace Broccolini.Tokenization;

internal interface ITokenizerRule
{
    Token? Match(ITokenizerInput input, IReadOnlyList<Token> context);
}
