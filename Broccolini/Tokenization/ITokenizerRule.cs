namespace Broccolini.Tokenization;

internal interface ITokenizerRule
{
    Option<Token> Match(ITokenizerInput input, IReadOnlyList<Token> context);
}
