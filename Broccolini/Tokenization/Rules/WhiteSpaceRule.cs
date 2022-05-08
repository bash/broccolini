namespace Broccolini.Tokenization.Rules;

internal sealed class WhiteSpaceRule : ITokenizerRule
{
    public Option<Token> Match(ITokenizerInput input, IReadOnlyList<Token> context)
        => Option.Some(input.ReadWhile(CharPredicates.IsWhitespace))
            .Where(static x => x.Length > 0)
            .Select(static x => new Token.WhiteSpace(x) as Token);
}
