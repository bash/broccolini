using Broccolini.Syntax;

namespace Broccolini.Parsing;

internal static class NodeCategorizer
{
    public static (NodeType, int) PeekNextNodeType(IParserInput input)
        => input.PeekIgnoreWhitespaceAndNewLines() switch
        {
            (IniToken.Epsilon, var pos) => (NodeType.Epsilon, pos),
            (var token, var pos) when IsSection(token) => (NodeType.Section, pos),
            (var token, var pos) when IsComment(token) => (NodeType.Comment, pos),
            (var token, var pos) when IsKeyValue(input, token, pos) => (NodeType.KeyValue, pos),
            (_, var pos) => (NodeType.Unrecognized, pos),
        };

    private static bool IsSection(IniToken token)
        => token is IniToken.OpeningBracket;

    private static bool IsComment(IniToken token)
        => token is IniToken.Semicolon;

    private static bool IsKeyValue(IParserInput input, IniToken token, int pos)
        => input.PeekRange()
            .Skip(pos)
            .Prepend(token)
            .TakeWhile(t => t is not IniToken.NewLine)
            .Any(t => t is IniToken.EqualsSign);
}

internal enum NodeType
{
    Section,
    Comment,
    KeyValue,
    Unrecognized,
    Epsilon,
}
