using System.Collections.Immutable;
using Broccolini.Syntax;
using Broccolini.Tokenization.Rules;

namespace Broccolini.Tokenization;

internal sealed class Tokenizer
{
    private static readonly IImmutableList<ITokenizerRule> Rules = ImmutableArray.Create<ITokenizerRule>(
        new LineBreakRule(),
        new WhiteSpaceRule(),
        new IdentifierRule(),
        new SimpleRule(';', new Token.Semicolon()),
        new SimpleRule('[', new Token.OpeningBracket()),
        new SimpleRule(']', new Token.ClosingBracket()),
        new SimpleRule('\'', new Token.SingleQuote()),
        new SimpleRule('\"', new Token.DoubleQuote()),
        new SimpleRule('=', new Token.EqualsSign()));

    public static IImmutableList<Token> Tokenize(string input)
        => Tokenize(new TokenizerInput(input), Rules);

    private static IImmutableList<Token> Tokenize(ITokenizerInput input, IReadOnlyList<ITokenizerRule> rules)
    {
        var tokens = ImmutableArray.CreateBuilder<Token>();

        while (input.Peek(out _))
        {
            var token = rules
                .Select(rule => rule.Match(input))
                .FirstOrDefault(result => result is not null)
                    ?? throw new InvalidOperationException("No matching rule found for token");
            tokens.Add(token);
        }

        return tokens.ToImmutable();
    }
}
