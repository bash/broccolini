using System.Collections.Immutable;
using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal static class Parser
{
    public static IniDocument Parse(IParserInput input)
    {
        var nodes = ImmutableArray.CreateBuilder<SectionChildNode>();
        var sections = ImmutableArray.CreateBuilder<SectionNode>();

        while (input.Peek() is not Token.Epsilon)
        {
            var node = ParseNode(input);

            // ParseNode already guarantees that after the first section only other sections are returned
            if (node is SectionNode sectionNode)
            {
                sections.Add(sectionNode);
            }
            else
            {
                nodes.Add((SectionChildNode)node);
            }
        }

        return new IniDocument(nodes.ToImmutable(), sections.ToImmutable());
    }

    private static IniNode ParseNode(IParserInput input)
        => IsSection(input)
            ? ParseSection(input)
            : ParseSectionChildNode(input);

    private static SectionChildNode ParseSectionChildNode(IParserInput input)
        => input switch
        {
            _ when IsComment(input) => ParseTrivia(input),
            _ when IsKeyValue(input) => ParseKeyValue(input),
            _ => ParseTrivia(input),
        };

    private static bool IsSection(IParserInput input)
        => input.PeekIgnoreWhitespace() is Token.OpeningBracket;

    private static bool IsComment(IParserInput input)
        => input.PeekIgnoreWhitespace() is Token.Semicolon;

    private static bool IsKeyValue(IParserInput input)
    {
        for (var lookAhead = 0; input.Peek(lookAhead) is not Token.NewLine and not Token.Epsilon; lookAhead++)
        {
            if (input.Peek(lookAhead) is Token.EqualsSign)
            {
                return true;
            }
        }

        return false;
    }

    private static IniNode ParseSection(IParserInput input)
    {
        var leadingTrivia = input.ReadOrEmpty(static t => t is Token.WhiteSpace);
        var openingBracketToken = input.Read();
        var triviaAfterOpeningBracket = input.ReadOrEmpty(static t => t is Token.WhiteSpace);
        var name = string.Concat(input.ReadWhileExcludeTrailingWhitespace(static t => t is not Token.ClosingBracket and not Token.NewLine));
        var triviaBeforeClosingBracket = input.ReadOrEmpty(static t => t is Token.WhiteSpace);
        var closingBracket = input.ReadOrNull(static t => t is Token.ClosingBracket);
        var trailingTrivia = input.ReadWhile(static t => t is not Token.NewLine);
        var newLine = input.ReadOrNull(static t => t is Token.NewLine);
        var children = ParseSectionChildren(input);
        return new SectionNode(name, children)
        {
            LeadingTrivia = new TriviaList(leadingTrivia),
            OpeningBracket = openingBracketToken,
            TriviaAfterOpeningBracket = new TriviaList(triviaAfterOpeningBracket),
            TriviaBeforeClosingBracket = new TriviaList(triviaBeforeClosingBracket),
            ClosingBracket = closingBracket,
            TrailingTrivia = new TriviaList(trailingTrivia),
            NewLine = newLine,
        };
    }

    private static KeyValueNode ParseKeyValue(IParserInput input)
    {
        var leadingTrivia = input.ReadOrEmpty(static t => t is Token.WhiteSpace);
        var key = string.Concat(input.ReadWhileExcludeTrailingWhitespace(static t => t is not Token.EqualsSign));
        var triviaBeforeEqualsSign = input.ReadOrEmpty(static t => t is Token.WhiteSpace);
        var equalsSign = input.Read();
        var triviaAfterEqualsSign = input.ReadOrEmpty(static t => t is Token.WhiteSpace);
        var (openingQuote, value, closingQuote) = ParseQuotedValue(input);
        var trailingTrivia = input.ReadWhile(static t => t is not Token.NewLine);
        var newLine = input.ReadOrNull(static t => t is Token.NewLine);
        return new KeyValueNode(key, value)
        {
            LeadingTrivia = new TriviaList(leadingTrivia),
            TriviaBeforeEqualsSign = new TriviaList(triviaBeforeEqualsSign),
            EqualsSign = equalsSign,
            TriviaAfterEqualsSign = new TriviaList(triviaAfterEqualsSign),
            OpeningQuote = openingQuote,
            ClosingQuote = closingQuote,
            TrailingTrivia = new TriviaList(trailingTrivia),
            NewLine = newLine,
        };
    }

    private static TriviaNode ParseTrivia(IParserInput input)
        => new(new TriviaList(input.ReadWhile(t => t is not Token.NewLine)))
        {
            NewLine = input.ReadOrNull(static t => t is Token.NewLine),
        };

    private static (Token?, string, Token?) ParseQuotedValue(IParserInput input)
    {
        var openingQuote = input.ReadOrNull(static t => t is Token.SingleQuote or Token.DoubleQuote);
        var value = string.Concat(ParseValue(input));
        var closingQuote = input.ReadOrNull(static t => t is Token.SingleQuote or Token.DoubleQuote);

        static string ToString(Token? token) => token?.ToString() ?? string.Empty;

        return openingQuote == closingQuote
            ? (openingQuote, value, closingQuote)
            : (null, ToString(openingQuote) + value + ToString(closingQuote), null);
    }

    private static IImmutableList<Token> ParseValue(IParserInput input)
    {
        var tokens = ImmutableArray.CreateBuilder<Token>();

        while (true)
        {
            if (input.PeekIgnoreWhitespace() is Token.NewLine or Token.Epsilon
                || (input.Peek() is Token.DoubleQuote or Token.SingleQuote
                    && input.PeekIgnoreWhitespace(1) is Token.NewLine or Token.Epsilon))
            {
                break;
            }

            tokens.Add(input.Read());
        }

        return tokens.ToImmutable();
    }

    private static IImmutableList<SectionChildNode> ParseSectionChildren(IParserInput input)
    {
        var nodes = ImmutableArray.CreateBuilder<SectionChildNode>();

        while (input.Peek() is not Token.Epsilon && !IsSection(input))
        {
            nodes.Add((SectionChildNode)ParseNode(input));
        }

        return nodes.ToImmutable();
    }
}
