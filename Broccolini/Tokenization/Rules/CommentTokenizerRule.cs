using Broccolini.Syntax;

namespace Broccolini.Tokenization.Rules;

internal sealed class CommentTokenizerRule : ITokenizerRule
{
    public Token? Match(ITokenizerInput input, IReadOnlyList<Token> context)
        => input.Peek(out var character) && character == ';' && IsActive(context)
            ? new Token.Comment(input.ReadWhile(static c => !CharPredicates.IsLineBreak(c)))
            : null;

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
