using System.Collections.Immutable;
using Broccolini.Tokenization.Rules;

namespace Broccolini.Tokenization;

internal sealed class Tokenizer
{
    private static readonly IImmutableList<ITokenizerRule> Rules = ImmutableArray.Create<ITokenizerRule>(
        new CommentTokenizerRule(),
        new LineBreakRule(),
        new WhiteSpaceRule(),
        new IdentifierRule(),
        new SimpleRule('[', new Token.OpeningBracket()),
        new SimpleRule(']', new Token.ClosingBracket()),
        new SimpleRule('\'', new Token.SingleQuotes()),
        new SimpleRule('\"', new Token.DoubleQuotes()),
        new SimpleRule('=', new Token.EqualsSign()));

    public static IImmutableList<Token> Tokenize(ITokenizerInput input)
        => Tokenize(input, Rules);

    private static IImmutableList<Token> Tokenize(ITokenizerInput input, IReadOnlyList<ITokenizerRule> rules)
    {
        var tokens = ImmutableArray.CreateBuilder<Token>();

        while (input.Peek().Match(none: false, some: True))
        {
            var token = rules
                .WhereSelect(rule => rule.Match(input, tokens))
                .FirstOrNone()
                .GetOrElse(() => throw new InvalidOperationException("No matching rule found for token"));
            tokens.Add(token);
        }

        return tokens.ToImmutable();
    }
}
