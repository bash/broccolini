using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal static class Parser
{
    public static IniDocument Parse(IParserInput input)
    {
        var nodes = ImmutableArray.CreateBuilder<SectionChildIniNode>();
        var sections = ImmutableArray.CreateBuilder<SectionIniNode>();

        while (input.Peek() is not IniToken.Epsilon)
        {
            var node = ParseNode(input);

            // ParseNode already guarantees that after the first section only other sections are returned
            if (node is SectionIniNode sectionNode)
            {
                sections.Add(sectionNode);
            }
            else
            {
                nodes.Add((SectionChildIniNode)node);
            }
        }

        return new IniDocument(nodes.ToImmutable(), sections.ToImmutable());
    }

    private static IniNode ParseNode(IParserInput input)
        => IsSection(input)
            ? ParseSection(input)
            : ParseSectionChildNode(input);

    private static SectionChildIniNode ParseSectionChildNode(IParserInput input)
        => input switch
        {
            _ when IsComment(input) => ParseComment(input),
            _ when IsKeyValue(input) => ParseKeyValue(input),
            _ => ParseTrivia(input),
        };

    private static bool IsSection(IParserInput input)
        => input.PeekIgnoreWhitespace() is IniToken.OpeningBracket;

    private static bool IsComment(IParserInput input)
        => input.PeekIgnoreWhitespace() is IniToken.Semicolon;

    private static bool IsKeyValue(IParserInput input)
    {
        for (var lookAhead = 0; input.Peek(lookAhead) is not IniToken.NewLine and not IniToken.Epsilon; lookAhead++)
        {
            if (input.Peek(lookAhead) is IniToken.EqualsSign)
            {
                return true;
            }
        }

        return false;
    }

    private static IniNode ParseSection(IParserInput input)
    {
        var leadingTrivia = input.ReadOrNull<IniToken.WhiteSpace>();
        var openingBracketToken = input.Read();
        var triviaAfterOpeningBracket = input.ReadOrNull<IniToken.WhiteSpace>();
        var name = string.Concat(input.ReadWhileExcludeTrailingWhitespace(static t => t is not IniToken.ClosingBracket and not IniToken.NewLine));
        var triviaBeforeClosingBracket = input.ReadOrNull<IniToken.WhiteSpace>();
        var closingBracket = input.ReadOrNull<IniToken.ClosingBracket>();
        var trailingTrivia = input.ReadWhile(static t => t is not IniToken.NewLine);
        var newLine = input.ReadOrNull<IniToken.NewLine>();
        var children = ParseSectionChildren(input);
        return new SectionIniNode(name, children)
        {
            LeadingTrivia = leadingTrivia,
            OpeningBracket = (IniToken.OpeningBracket)openingBracketToken,
            TriviaAfterOpeningBracket = triviaAfterOpeningBracket,
            TriviaBeforeClosingBracket = triviaBeforeClosingBracket,
            ClosingBracket = closingBracket,
            TrailingTrivia = trailingTrivia,
            NewLine = newLine,
        };
    }

    private static KeyValueIniNode ParseKeyValue(IParserInput input)
    {
        var leadingTrivia = input.ReadOrNull<IniToken.WhiteSpace>();
        var key = string.Concat(input.ReadWhileExcludeTrailingWhitespace(static t => t is not IniToken.EqualsSign));
        var triviaBeforeEqualsSign = input.ReadOrNull<IniToken.WhiteSpace>();
        var equalsSign = input.Read();
        var triviaAfterEqualsSign = input.ReadOrNull<IniToken.WhiteSpace>();
        var (quote, value) = ParseQuotedValue(input);
        var trailingTrivia = input.ReadOrNull<IniToken.WhiteSpace>();
        var newLine = input.ReadOrNull<IniToken.NewLine>();
        return new KeyValueIniNode(key, value)
        {
            LeadingTrivia = leadingTrivia,
            TriviaBeforeEqualsSign = triviaBeforeEqualsSign,
            EqualsSign = (IniToken.EqualsSign)equalsSign,
            TriviaAfterEqualsSign = triviaAfterEqualsSign,
            Quote = quote,
            TrailingTrivia = trailingTrivia,
            NewLine = newLine,
        };
    }

    private static CommentIniNode ParseComment(IParserInput input)
    {
        var leadingTrivia = input.ReadOrNull<IniToken.WhiteSpace>();
        var semicolon = (IniToken.Semicolon)input.Read();
        var triviaAfterSemicolon = input.ReadOrNull<IniToken.WhiteSpace>();
        var text = string.Concat(input.ReadWhileExcludeTrailingWhitespace(static t => t is not IniToken.NewLine));
        var trailingTrivia = input.ReadOrNull<IniToken.WhiteSpace>();
        var newLine = input.ReadOrNull<IniToken.NewLine>();
        return new CommentIniNode(text)
        {
            LeadingTrivia = leadingTrivia,
            Semicolon = semicolon,
            TriviaAfterSemicolon = triviaAfterSemicolon,
            TrailingTrivia = trailingTrivia,
            NewLine = newLine,
        };
    }

    private static UnrecognizedIniNode ParseTrivia(IParserInput input)
        => new(input.ReadWhile(t => t is not IniToken.NewLine))
        {
            NewLine = input.ReadOrNull<IniToken.NewLine>(),
        };

    private static (IniToken.Quote?, string) ParseQuotedValue(IParserInput input)
    {
        var openingQuote = input.ReadOrNull<IniToken.Quote>(static t => t is IniToken.SingleQuote or IniToken.DoubleQuote);
        var value = string.Concat(ParseValue(input));
        var closingQuote = input.ReadOrNull<IniToken.Quote>(static t => t is IniToken.SingleQuote or IniToken.DoubleQuote);

        static string ToString(IniToken? token) => token?.ToString() ?? string.Empty;

        return openingQuote == closingQuote
            ? (openingQuote, value)
            : (null, ToString(openingQuote) + value + ToString(closingQuote));
    }

    private static IImmutableList<IniToken> ParseValue(IParserInput input)
    {
        var tokens = ImmutableArray.CreateBuilder<IniToken>();

        while (true)
        {
            if (input.PeekIgnoreWhitespace() is IniToken.NewLine or IniToken.Epsilon
                || (input.Peek() is IniToken.DoubleQuote or IniToken.SingleQuote
                    && input.PeekIgnoreWhitespace(1) is IniToken.NewLine or IniToken.Epsilon))
            {
                break;
            }

            tokens.Add(input.Read());
        }

        return tokens.ToImmutable();
    }

    private static IImmutableList<SectionChildIniNode> ParseSectionChildren(IParserInput input)
    {
        var nodes = ImmutableArray.CreateBuilder<SectionChildIniNode>();

        while (input.Peek() is not IniToken.Epsilon && !IsSection(input))
        {
            nodes.Add((SectionChildIniNode)ParseNode(input));
        }

        return nodes.ToImmutable();
    }
}
