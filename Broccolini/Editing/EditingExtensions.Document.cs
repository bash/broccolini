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
            var section = (SectionNode)children[index];
            var trailingTrivia = section.Children.Reverse().TakeWhile(n => n is TriviaNode).Reverse();
            children = children.RemoveAt(index);
            children = index >= 1 && children[index - 1] is SectionNode previousSectionNode
                ? children.SetItem(index - 1, previousSectionNode with { Children = previousSectionNode.Children.AddRange(trailingTrivia) })
                : children.InsertRange(index, trailingTrivia);
        }

        return document with { Children = children };
    }

    private static IniDocument AppendChild(this IniDocument document, IniNode node)
    {
        var documentWithNewLine = document.EnsureTrailingNewLine(DefaultNewLine);
        return documentWithNewLine with { Children = documentWithNewLine.Children.Add(node) };
    }
}
