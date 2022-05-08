namespace Broccolini.Tokenization.Rules;

internal sealed class IdentifierRule : ITokenizerRule
{
    public Option<Token> Match(ITokenizerInput input, IReadOnlyList<Token> context)
        => Option.Some(input.ReadWhile(CharPredicates.IsIdentifier))
            .Where(static x => x.Length > 0)
            .Select(static x => new Token.Identifier(x) as Token);
}
