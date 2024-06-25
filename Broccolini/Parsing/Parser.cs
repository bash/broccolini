using Broccolini.Syntax;
using static Broccolini.Parsing.NodeCategorizer;
using static Broccolini.Parsing.TriviaReader;

namespace Broccolini.Parsing;

internal static class Parser
{
    public static IniDocument Parse(IParserInput input)
    {
        var nodes = ImmutableArray.CreateBuilder<SectionChildIniNode>();
        var sections = ImmutableArray.CreateBuilder<SectionIniNode>();

        while (input.Peek() is not IniToken.Epsilon)
        {
            var node = ParseNode(input, TriviaParseContext.TopLevelSectionChild);

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

    private static IniNode ParseNode(IParserInput input, TriviaParseContext context)
    {
        var (nodeType, _) = PeekNextNodeType(input);
        return ParseNode(input, nodeType, context);
    }

    private static IniNode ParseNode(IParserInput input, NodeType nodeType, TriviaParseContext context)
        => nodeType switch
        {
            NodeType.Section => ParseSection(input),
            NodeType.Comment => ParseComment(input, context),
            NodeType.KeyValue => ParseKeyValue(input, context),
            // In a document consisting just of whitespace we get one epsilon node
            NodeType.Unrecognized or NodeType.Epsilon => ParseUnrecognized(input, context),
            _ => throw new InvalidOperationException("Unreachable: unrecognized node type (this is a bug)"),
        };

    private static IniNode ParseSection(IParserInput input)
    {
        var leadingTrivia = input.Read(PeekLeadingTrivia(input).DropLast(t => t is IniToken.WhiteSpace));
        var header = ParseSectionHeader(input);
        var children = ParseSectionChildren(input);
        var trailingTrivia = ReadTrailingSectionTrivia(input);
        return new SectionIniNode(header, children)
        {
            LeadingTrivia = leadingTrivia,
            TrailingTrivia = trailingTrivia,
        };
    }

    private static SectionHeaderIniNode ParseSectionHeader(IParserInput input)
    {
        var leadingTrivia = input.ReadWhile(static t => t is IniToken.WhiteSpace);
        var openingBracketToken = input.Read();
        var triviaAfterOpeningBracket = input.ReadOrNull<IniToken.WhiteSpace>();
        var name = string.Concat(input.ReadWhileExcludeTrailingWhitespace(static t => t is not IniToken.ClosingBracket and not IniToken.NewLine));
        var triviaBeforeClosingBracket = input.ReadOrNull<IniToken.WhiteSpace>();
        var closingBracket = input.ReadOrNull<IniToken.ClosingBracket>();
        var unrecognizedTokensAfterClosingBracket = input.Read(input.PeekRange().TakeWhile(t => t is not IniToken.NewLine).DropLast(t => t is IniToken.WhiteSpace));
        var trailingTrivia = ReadTrailingTrivia(input, TriviaParseContext.SectionChild);
        var newLine = input.ReadOrNull<IniToken.NewLine>();
        return new SectionHeaderIniNode(name)
        {
            LeadingTrivia = leadingTrivia,
            OpeningBracket = (IniToken.OpeningBracket)openingBracketToken,
            TriviaAfterOpeningBracket = triviaAfterOpeningBracket,
            TriviaBeforeClosingBracket = triviaBeforeClosingBracket,
            ClosingBracket = closingBracket,
            UnrecognizedTokensAfterClosingBracket = unrecognizedTokensAfterClosingBracket,
            TrailingTrivia = trailingTrivia,
            NewLine = newLine,
        };
    }

    private static KeyValueIniNode ParseKeyValue(IParserInput input, TriviaParseContext context)
    {
        var leadingTrivia = ReadLeadingTrivia(input);
        var key = string.Concat(input.ReadWhileExcludeTrailingWhitespace(static t => t is not IniToken.EqualsSign));
        var triviaBeforeEqualsSign = input.ReadOrNull<IniToken.WhiteSpace>();
        var equalsSign = input.Read();
        var triviaAfterEqualsSign = input.ReadOrNull<IniToken.WhiteSpace>();
        var (quote, value) = ParseQuotedValue(input);
        var trailingTrivia = ReadTrailingTrivia(input, context);
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

    private static CommentIniNode ParseComment(IParserInput input, TriviaParseContext context)
    {
        var leadingTrivia = ReadLeadingTrivia(input);
        var semicolon = (IniToken.Semicolon)input.Read();
        var triviaAfterSemicolon = input.ReadOrNull<IniToken.WhiteSpace>();
        var text = string.Concat(input.ReadWhileExcludeTrailingWhitespace(static t => t is not IniToken.NewLine));
        var trailingTrivia = ReadTrailingTrivia(input, context);
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

    private static UnrecognizedIniNode ParseUnrecognized(IParserInput input, TriviaParseContext context)
    {
        var leadingTrivia = ReadLeadingTrivia(input);
        var content = input.ReadWhileExcludeTrailingWhitespace(t => t is not IniToken.NewLine);
        var trailingTrivia = ReadTrailingTrivia(input, context);
        var newLine = input.ReadOrNull<IniToken.NewLine>();
        return new UnrecognizedIniNode(content)
        {
            LeadingTrivia = leadingTrivia,
            NewLine = newLine,
            TrailingTrivia = trailingTrivia,
        };
    }

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

        while (PeekNextNodeType(input) is (var type, _) && type is not (NodeType.Epsilon or NodeType.Section))
        {
            nodes.Add((SectionChildIniNode)ParseNode(input, type, TriviaParseContext.SectionChild));
        }

        return nodes.ToImmutable();
    }
}
