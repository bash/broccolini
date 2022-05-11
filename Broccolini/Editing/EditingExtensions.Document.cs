using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Broccolini.Syntax;
using static Broccolini.Syntax.SyntaxFactory;

namespace Broccolini.Editing;

public static partial class EditingExtensions
{
    private static Token.LineBreak DefaultNewLine = new(Environment.NewLine);

    /// <summary>Appends or updates a section with the given name.</summary>
    [Pure]
    public static IniDocument WithSection(this IniDocument document, string sectionName, Func<SectionNode, SectionNode> updateSection)
        => document.Children.TryUpdateFirst(
            node => node is SectionNode { Name: var name } && name == sectionName,
            node => updateSection((SectionNode)node),
            out var updatedChildren)
                ? document with { Children = updatedChildren }
                : document.AppendChild(updateSection(Section(sectionName)));

    /// <summary>Updates a section with the given name. Does nothing when the section does not exist.</summary>
    [Pure]
    public static IniDocument UpdateSection(this IniDocument document, string sectionName, Func<SectionNode, SectionNode> updateSection)
        => document.Children.TryUpdateFirst(
            node => node is SectionNode { Name: var name } && name == sectionName,
            node => updateSection((SectionNode)node),
            out var updatedChildren)
                ? document with { Children = updatedChildren }
                : document;

    /// <summary>Removes all sections with the given name. Preserves trailing trivia.</summary>
    [Pure]
    public static IniDocument RemoveSection(this IniDocument document, string sectionName)
    {
        var children = document.Children;

        while (children.TryFindIndex(node => node is SectionNode { Name: var name } && name == sectionName, out var index))
        {
            var trailingTrivia = GetTrailingTrivia((SectionNode)children[index]);
            children = children.RemoveAt(index);
            children = InsertNodesAt(children, index, trailingTrivia);
        }

        return document with { Children = children };

        static IEnumerable<SectionChildNode> GetTrailingTrivia(SectionNode sectionNode)
            => sectionNode.Children.Reverse().TakeWhile(n => n is TriviaNode).Reverse();

        static IImmutableList<IniNode> InsertNodesAt(IImmutableList<IniNode> nodes, int index, IEnumerable<SectionChildNode> nodesToInsert)
        {
            var previousIndex = index - 1;
            return index >= 1 && nodes[previousIndex] is SectionNode previousSectionNode
                ? nodes.SetItem(previousIndex, previousSectionNode with { Children = previousSectionNode.Children.AddRange(nodesToInsert) })
                : nodes.InsertRange(index, nodesToInsert);
        }
    }

    private static IniDocument AppendChild(this IniDocument document, IniNode node)
    {
        var documentWithNewLine = document.EnsureTrailingNewLine(DefaultNewLine);
        return documentWithNewLine with { Children = documentWithNewLine.Children.Add(node) };
    }
}
