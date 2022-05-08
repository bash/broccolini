using Broccolini.Syntax;

namespace Broccolini.Tokenization.Rules;

internal sealed class LineBreakRule : ITokenizerRule
{
    public Option<Token> Match(ITokenizerInput input, IReadOnlyList<Token> context)
    {
        if (input.Peek() == '\r' && input.Peek(1) == '\n')
        {
            input.Read();
            input.Read();
            return new Token.LineBreak("\r\n");
        }

        if (input.Peek().Match(none: false, some: CharPredicates.IsLineBreak))
        {
            return new Token.LineBreak(input.Read().ToString());
        }

        return Option<Token>.None();
    }
}
