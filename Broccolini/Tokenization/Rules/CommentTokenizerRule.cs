using Broccolini.Syntax;

namespace Broccolini.Tokenization.Rules;

internal sealed class CommentTokenizerRule : ITokenizerRule
{
    public Option<Token> Match(ITokenizerInput input, IReadOnlyList<Token> context)
        => input.Peek() == ';' && IsActive(context)
            ? new Token.Comment(input.ReadWhile(static c => !CharPredicates.IsLineBreak(c)))
            : Option<Token>.None();

    private static bool IsActive(IReadOnlyList<Token> context)
        => context.Count switch
        {
            0 => true,
            1 when context[0] is Token.WhiteSpace => true,
            >= 1 when context[context.Count - 1] is Token.LineBreak => true,
            >= 2 when context[context.Count - 2] is Token.LineBreak && context[context.Count - 1] is Token.WhiteSpace => true,
            _ => false,
        };
}
