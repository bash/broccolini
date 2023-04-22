using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using Broccolini.Tokenization.Rules;
using static Broccolini.Tokenization.Rules.CharPredicates;
using static Broccolini.Tokenization.Tokenizer;

namespace Broccolini.Syntax;

[EditorBrowsable(EditorBrowsableState.Advanced)]
public static class IniSyntaxFactory
{
    private static readonly IniToken.WhiteSpace Space = new(" ");

    /// <summary>Creates a key-value node while taking care of properly quoting the <paramref name="value"/> as needed.</summary>
    /// <param name="key">The key may contain anything except newlines, <c>=</c>, leading <c>[</c> or <c>;</c>, and leading or trailing whitespace.</param>
    /// <param name="value">The value may contain anything except newlines. Quotes are automatically added as needed to preserve whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when the created node would result in something different when parsed back.</exception>
    [Pure]
    public static KeyValueIniNode KeyValue(string key, string value)
    {
        var tokenizedValue = Tokenize(value);

        ValidateKey(key);
        ValidateValue(value, tokenizedValue);

        var quote = ShouldAddQuotesAroundValue(tokenizedValue)
            ? new IniToken.DoubleQuote()
            : null;

        return new KeyValueIniNode(key, value)
        {
            TriviaBeforeEqualsSign = Space,
            TriviaAfterEqualsSign = Space,
            Quote = quote,
        };
    }

    /// <summary>Creates a section node while validating that the <paramref name="name"/> is valid.</summary>
    /// <param name="name">The name may contain anything except newlines, <c>]</c>, and leading or trailing whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when the created node would result in something different when parsed back.</exception>
    [Pure]
    public static SectionIniNode Section(string name)
    {
        ValidateSectionName(name);
        return new SectionIniNode(name, ImmutableArray<SectionChildIniNode>.Empty);
    }

    /// <summary>Creates a whitespace token.</summary>
    /// <exception cref="ArgumentException">Thrown when the value is not valid whitespace.</exception>
    [Pure]
    public static IniToken.WhiteSpace WhiteSpace(string value)
        => value.All(IsWhitespace)
            ? new IniToken.WhiteSpace(value)
            : throw new ArgumentException($"'{value}' is not valid whitespace", nameof(value));

    /// <summary>Creates a newline token.</summary>
    /// <exception cref="ArgumentException">Thrown when the value is not a newline.</exception>
    [Pure]
    public static IniToken.NewLine NewLine(string value)
        => value is "\r" or "\n" or "\r\n"
            ? new IniToken.NewLine(value)
            : throw new ArgumentException($"'{value}' is not a valid newline");

    /// <summary>Creates a identifier token.</summary>
    /// <exception cref="ArgumentException">Thrown when the value is not a valid identifier.</exception>
    [Pure]
    public static IniToken.Identifier Identifier(string value)
        => value.All(IsIdentifier)
            ? new IniToken.Identifier(value)
            : throw new ArgumentException($"'{value}' is not a valid identifier");

    private static void ValidateSectionName(string name)
    {
        var tokens = Tokenize(name);

        if (tokens.Any(static t => t is IniToken.NewLine or IniToken.ClosingBracket))
        {
            throw new ArgumentException($"Section name '{name}' contains one ore more invalid characters: line breaks and ] are not allowed", nameof(name));
        }

        if (tokens is [IniToken.WhiteSpace, ..] or [.., IniToken.WhiteSpace])
        {
            throw new ArgumentException($"Section name '{name}' contains leading or trailing whitespace, which will not be preserved", nameof(name));
        }
    }

    private static void ValidateKey(string key)
    {
        var tokens = Tokenize(key);

        if (tokens.Any(static t => t is IniToken.NewLine or IniToken.EqualsSign))
        {
            throw new ArgumentException($"Key '{key}' contains one ore more invalid characters: line breaks and = are not allowed", nameof(key));
        }

        if (tokens is [IniToken.OpeningBracket, ..] or [IniToken.WhiteSpace, IniToken.OpeningBracket, ..])
        {
            throw new ArgumentException($"Key '{key}' may not start with an opening bracket", nameof(key));
        }

        if (tokens is [IniToken.Semicolon, ..] or [IniToken.WhiteSpace, IniToken.Semicolon, ..])
        {
            throw new ArgumentException($"Key '{key}' may not start with a semicolon", nameof(key));
        }

        if (tokens is [IniToken.WhiteSpace, ..] or [.., IniToken.WhiteSpace])
        {
            throw new ArgumentException($"Key '{key}' contains leading or trailing whitespace, which will not be preserved", nameof(key));
        }
    }

    private static void ValidateValue(string value, IReadOnlyList<IniToken> tokens)
    {
        if (tokens.Any(static t => t is IniToken.NewLine))
        {
            throw new ArgumentException($"Value '{value}' contains one ore more invalid characters: line breaks are not allowed", nameof(value));
        }
    }

    private static bool ShouldAddQuotesAroundValue(IReadOnlyList<IniToken> tokens)
    {
        bool HasLeadingOrTrailingWhitespace()
            => tokens is [IniToken.WhiteSpace, ..] or [.., IniToken.WhiteSpace];

        bool IsQuoted()
            => tokens is [IniToken.DoubleQuote, .., IniToken.DoubleQuote] or [IniToken.SingleQuote, .., IniToken.SingleQuote];

        return HasLeadingOrTrailingWhitespace() || IsQuoted();
    }
}
