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
            _ when IsComment(input) => ParseComment(input),
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
        var leadingTrivia = input.ReadOrNull<Token.WhiteSpace>();
        var openingBracketToken = input.Read();
        var triviaAfterOpeningBracket = input.ReadOrNull<Token.WhiteSpace>();
        var name = string.Concat(input.ReadWhileExcludeTrailingWhitespace(static t => t is not Token.ClosingBracket and not Token.NewLine));
        var triviaBeforeClosingBracket = input.ReadOrNull<Token.WhiteSpace>();
        var closingBracket = input.ReadOrNull<Token.ClosingBracket>();
        var trailingTrivia = input.ReadWhile(static t => t is not Token.NewLine);
        var newLine = input.ReadOrNull<Token.NewLine>();
        var children = ParseSectionChildren(input);
        return new SectionNode(name, children)
        {
            LeadingTrivia = leadingTrivia,
            OpeningBracket = (Token.OpeningBracket)openingBracketToken,
            TriviaAfterOpeningBracket = triviaAfterOpeningBracket,
            TriviaBeforeClosingBracket = triviaBeforeClosingBracket,
            ClosingBracket = closingBracket,
            TrailingTrivia = trailingTrivia,
            NewLine = newLine,
        };
    }

    private static KeyValueNode ParseKeyValue(IParserInput input)
    {
        var leadingTrivia = input.ReadOrNull<Token.WhiteSpace>();
        var key = string.Concat(input.ReadWhileExcludeTrailingWhitespace(static t => t is not Token.EqualsSign));
        var triviaBeforeEqualsSign = input.ReadOrNull<Token.WhiteSpace>();
        var equalsSign = input.Read();
        var triviaAfterEqualsSign = input.ReadOrNull<Token.WhiteSpace>();
        var (quote, value) = ParseQuotedValue(input);
        var trailingTrivia = input.ReadOrNull<Token.WhiteSpace>();
        var newLine = input.ReadOrNull<Token.NewLine>();
        return new KeyValueNode(key, value)
        {
            LeadingTrivia = leadingTrivia,
            TriviaBeforeEqualsSign = triviaBeforeEqualsSign,
            EqualsSign = (Token.EqualsSign)equalsSign,
            TriviaAfterEqualsSign = triviaAfterEqualsSign,
            Quote = quote,
            TrailingTrivia = trailingTrivia,
            NewLine = newLine,
        };
    }

    private static CommentNode ParseComment(IParserInput input)
    {
        var leadingTrivia = input.ReadOrNull<Token.WhiteSpace>();
        var semicolon = (Token.Semicolon)input.Read();
        var triviaAfterSemicolon = input.ReadOrNull<Token.WhiteSpace>();
        var text = string.Concat(input.ReadWhileExcludeTrailingWhitespace(static t => t is not Token.NewLine));
        var trailingTrivia = input.ReadOrNull<Token.WhiteSpace>();
        var newLine = input.ReadOrNull<Token.NewLine>();
        return new CommentNode(text)
        {
            LeadingTrivia = leadingTrivia,
            Semicolon = semicolon,
            TriviaAfterSemicolon = triviaAfterSemicolon,
            TrailingTrivia = trailingTrivia,
            NewLine = newLine,
        };
    }

    private static UnrecognizedNode ParseTrivia(IParserInput input)
        => new(input.ReadWhile(t => t is not Token.NewLine))
        {
            NewLine = input.ReadOrNull<Token.NewLine>(),
        };

    private static (Token.Quote?, string) ParseQuotedValue(IParserInput input)
    {
        var openingQuote = input.ReadOrNull<Token.Quote>(static t => t is Token.SingleQuote or Token.DoubleQuote);
        var value = string.Concat(ParseValue(input));
        var closingQuote = input.ReadOrNull<Token.Quote>(static t => t is Token.SingleQuote or Token.DoubleQuote);

        static string ToString(Token? token) => token?.ToString() ?? string.Empty;

        return openingQuote == closingQuote
            ? (openingQuote, value)
            : (null, ToString(openingQuote) + value + ToString(closingQuote));
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
