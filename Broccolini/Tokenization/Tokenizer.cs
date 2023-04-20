using System.Collections.Immutable;
using Broccolini.Syntax;
using Broccolini.Tokenization.Rules;

namespace Broccolini.Tokenization;

internal sealed class Tokenizer
{
    private static readonly IImmutableList<ITokenizerRule> Rules = ImmutableArray.Create<ITokenizerRule>(
        new NewLineRule(),
        new WhiteSpaceRule(),
        new IdentifierRule(),
        new SimpleRule(';', new IniToken.Semicolon()),
        new SimpleRule('[', new IniToken.OpeningBracket()),
        new SimpleRule(']', new IniToken.ClosingBracket()),
        new SimpleRule('\'', new IniToken.SingleQuote()),
        new SimpleRule('\"', new IniToken.DoubleQuote()),
        new SimpleRule('=', new IniToken.EqualsSign()));

    public static IImmutableList<IniToken> Tokenize(string input)
        => Tokenize(new TokenizerInput(input), Rules);

    private static IImmutableList<IniToken> Tokenize(ITokenizerInput input, IReadOnlyList<ITokenizerRule> rules)
    {
        var tokens = ImmutableArray.CreateBuilder<IniToken>();

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
