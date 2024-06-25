using Broccolini.Syntax;
using static Broccolini.Parsing.NodeCategorizer;

namespace Broccolini.Parsing;

// See doc/trivia-explainer.svg for a visual explanation
// of how trivia is attributed to nodes.
internal static class TriviaReader
{
    public static ImmutableArray<IniToken> ReadTrailingTrivia(IParserInput input, TriviaParseContext context)
    {
        var (next, triviaLength) = PeekNextNodeType(input);
        var triviaInput = input.PeekRange().Take(triviaLength);

        var trivia = (triviaLength, next) switch
        {
            // trailing blank lines belong to the section's trailing trivia
            (_, NodeType.Section or NodeType.Epsilon) when context is TriviaParseContext.SectionChild
                => triviaInput.TakeWhile(static t => t is not IniToken.NewLine),
            // the final newline is not part of trivia
            (>=1, _) when input.Peek(triviaLength - 1) is IniToken.NewLine
                => triviaInput.SkipLast(1),
            // whitespace following a newline is leading trivia for the next node
            // the final newline is not part of trivia
            (>=2, not NodeType.Epsilon) when input.Peek(triviaLength - 2) is IniToken.NewLine && input.Peek(triviaLength - 1) is IniToken.WhiteSpace
                => triviaInput.SkipLast(2),
            _ => triviaInput,
        };

        return input.Read(trivia);
    }

    public static ImmutableArray<IniToken> ReadTrailingSectionTrivia(IParserInput input)
    {
        var (next, triviaLength) = PeekNextNodeType(input);
        var triviaInput = input.PeekRange().Take(triviaLength);

        var trivia = (triviaLength, next) switch
        {
            // a single whitespace means that the child node (header or key-value) has already consumed a newline
            // => this whitespace belongs to the next node
            (1, not NodeType.Epsilon) when input.Peek() is IniToken.WhiteSpace => [],
            // whitespace following a newline is leading trivia for the next node
            (>=2, not NodeType.Epsilon) when input.Peek(triviaLength - 2) is IniToken.NewLine && input.Peek(triviaLength - 1) is IniToken.WhiteSpace
                => triviaInput.SkipLast(1),
            _ => triviaInput,
        };

        return input.Read(trivia);
    }

    public static IImmutableList<IniToken> ReadLeadingTrivia(IParserInput input)
        => input.Read(PeekLeadingTrivia(input));

    public static IEnumerable<IniToken> PeekLeadingTrivia(IParserInput input)
        => input.PeekRange().TakeWhile(t => t is IniToken.WhiteSpace or IniToken.NewLine);
}

internal enum TriviaParseContext
{
    TopLevelSectionChild,
    SectionChild,
}
