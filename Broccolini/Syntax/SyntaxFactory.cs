using System.Collections.Immutable;
using static Broccolini.Tokenization.Tokenizer;

namespace Broccolini.Syntax;

public static class SyntaxFactory
{
    private static readonly Token.WhiteSpace Space = new(" ");

    /// <summary>Creates a key-value node while taking care of properly quoting the <paramref name="value"/> as needed.</summary>
    /// <param name="key">The key may contain anything except newlines, <c>=</c>, leading <c>[</c> or <c>;</c>, and leading or trailing whitespace.</param>
    /// <param name="value">The value may contain anything except newlines. Quotes are automatically added as needed to preserve whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when the created node would result in something different when parsed back.</exception>
    public static KeyValueNode KeyValue(string key, string value)
    {
        var tokenizedValue = Tokenize(value);

        ValidateKey(key);
        ValidateValue(value, tokenizedValue);

        var quote = ShouldAddQuotesAroundValue(tokenizedValue)
            ? new Token.DoubleQuote()
            : null;

        return new KeyValueNode(key, value)
        {
            TriviaBeforeEqualsSign = Space,
            TriviaAfterEqualsSign = Space,
            Quote = quote,
        };
    }

    /// <summary>Creates a section node while validating that the <paramref name="name"/> is valid.</summary>
    /// <param name="name">The name may contain anything except newlines, <c>]</c>, and leading or trailing whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when the created node would result in something different when parsed back.</exception>
    public static SectionNode Section(string name)
    {
        ValidateSectionName(name);
        return new SectionNode(name, ImmutableArray<SectionChildNode>.Empty);
    }

    private static void ValidateSectionName(string name)
    {
        var tokens = Tokenize(name);

        if (tokens.Any(static t => t is Token.NewLine or Token.ClosingBracket))
        {
            throw new ArgumentException($"Section name '{name}' contains one ore more invalid characters: line breaks and ] are not allowed", nameof(name));
        }

        if (tokens is [Token.WhiteSpace, ..] or [.., Token.WhiteSpace])
        {
            throw new ArgumentException($"Section name '{name}' contains leading or trailing whitespace, which will not be preserved", nameof(name));
        }
    }

    private static void ValidateKey(string key)
    {
        var tokens = Tokenize(key);

        if (tokens.Any(static t => t is Token.NewLine or Token.EqualsSign))
        {
            throw new ArgumentException($"Key '{key}' contains one ore more invalid characters: line breaks and = are not allowed", nameof(key));
        }

        if (tokens is [Token.OpeningBracket, ..] or [Token.WhiteSpace, Token.OpeningBracket, ..])
        {
            throw new ArgumentException($"Key '{key}' may not start with an opening bracket", nameof(key));
        }

        if (tokens is [Token.Semicolon, ..] or [Token.WhiteSpace, Token.Semicolon, ..])
        {
            throw new ArgumentException($"Key '{key}' may not start with a semicolon", nameof(key));
        }

        if (tokens is [Token.WhiteSpace, ..] or [.., Token.WhiteSpace])
        {
            throw new ArgumentException($"Key '{key}' contains leading or trailing whitespace, which will not be preserved", nameof(key));
        }
    }

    private static void ValidateValue(string value, IReadOnlyList<Token> tokens)
    {
        if (tokens.Any(static t => t is Token.NewLine))
        {
            throw new ArgumentException($"Value '{value}' contains one ore more invalid characters: line breaks are not allowed", nameof(value));
        }
    }

    private static bool ShouldAddQuotesAroundValue(IReadOnlyList<Token> tokens)
    {
        bool HasLeadingOrTrailingWhitespace()
            => tokens is [Token.WhiteSpace, ..] or [.., Token.WhiteSpace];

        bool IsQuoted()
            => tokens is [Token.DoubleQuote, .., Token.DoubleQuote] or [Token.SingleQuote, .., Token.SingleQuote];

        return HasLeadingOrTrailingWhitespace() || IsQuoted();
    }
}
