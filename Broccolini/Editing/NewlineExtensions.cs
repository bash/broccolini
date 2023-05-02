using Broccolini.Syntax;

namespace Broccolini.Editing;

internal static class NewLineExtensions
{
    public static IniDocument EnsureTrailingNewLine(this IniDocument document, IniToken.NewLine newLine)
        => document switch
        {
            { Sections: { Count: >=1 } sections } => document with { Sections = sections.ReplaceLast(n => EnsureTrailingNewLine(n, newLine)) },
            { NodesOutsideSection: { Count: >= 1 } nodes } => document with { NodesOutsideSection = nodes.ReplaceLast(n => EnsureTrailingNewLine(n, newLine)) },
            _ => document,
        };

    public static SectionIniNode EnsureTrailingNewLine(this SectionIniNode node, IniToken.NewLine newLine)
        => node switch
        {
            { Children: { Count: >=1 } children } => node with { Children = children.ReplaceLast(n => EnsureTrailingNewLine(n, newLine)) },
            { Children.Count: 0, NewLine: null } => node with { NewLine = newLine },
            _ => node,
        };

    public static SectionChildIniNode EnsureTrailingNewLine(this SectionChildIniNode node, IniToken.NewLine newLine)
        => node.NewLine is null
            ? node with { NewLine = newLine }
            : node;

    private static IImmutableList<T> ReplaceLast<T>(this IImmutableList<T> list, Func<T, T> update)
        => list.SetItem(list.Count - 1, update(list[list.Count - 1]));
}
