using System.Diagnostics.Contracts;
using Broccolini.Syntax;
using static Broccolini.SemanticModel.KeyComparision;
using static Broccolini.Syntax.SyntaxFactory;

namespace Broccolini.Editing;

public static partial class EditingExtensions
{
    /// <summary>Appends or updates a section with the given name.</summary>
    [Pure]
    public static IniDocument WithSection(this IniDocument document, string sectionName, Func<SectionNode, SectionNode> updateSection)
        => document.Sections.TryUpdateFirst(
            section => KeyEquals(section.Name, sectionName),
            EnsureTrailingNewLine(WithNewLineHint(updateSection, document), document),
            out var updatedSections)
                ? document with { Sections = updatedSections }
                : document.AppendSection(updateSection(Section(sectionName)));

    /// <summary>Updates a section with the given name. Does nothing when the section does not exist.</summary>
    [Pure]
    public static IniDocument UpdateSection(this IniDocument document, string sectionName, Func<SectionNode, SectionNode> updateSection)
        => document.Sections.TryUpdateFirst(
            section => KeyEquals(section.Name, sectionName),
            EnsureTrailingNewLine(WithNewLineHint(updateSection, document), document),
            out var updatedSections)
                ? document with { Sections = updatedSections }
                : document;

    /// <summary>Removes all sections with the given name. Preserves trailing trivia.</summary>
    [Pure]
    public static IniDocument RemoveSection(this IniDocument document, string sectionName)
    {
        while (document.Sections.TryFindIndex(section => KeyEquals(section.Name, sectionName), out var index))
        {
            var trailingTrivia = GetTrailingTrivia(document.Sections[index]);
            document = document with { Sections = document.Sections.RemoveAt(index) };
            document = InsertNodesAt(document, index, trailingTrivia);
        }

        return document;

        static IEnumerable<SectionChildNode> GetTrailingTrivia(SectionNode sectionNode)
            => sectionNode.Children.Reverse().TakeWhile(n => n is UnrecognizedNode or CommentNode).Reverse();

        static IniDocument InsertNodesAt(IniDocument document, int index, IEnumerable<SectionChildNode> nodesToInsert)
            => index >= 1 && document.Sections[index - 1] is var previousSection
                ? document with { Sections = document.Sections.SetItem(index - 1, previousSection with { Children = previousSection.Children.AddRange(nodesToInsert) }) }
                : document with { NodesOutsideSection = document.NodesOutsideSection.InsertRange(index, nodesToInsert) };
    }

    private static IniDocument AppendSection(this IniDocument document, SectionNode node)
    {
        var documentWithNewLine = document.EnsureTrailingNewLine(document.DetectNewLine());
        return documentWithNewLine with { Sections = documentWithNewLine.Sections.Add(node) };
    }

    private static Func<SectionNode, SectionNode> WithNewLineHint(Func<SectionNode, SectionNode> updateSection, IniDocument document)
        => section => updateSection(section with { NewLineHint = document.DetectNewLine() }) with { NewLineHint = null };

    private static Func<SectionNode, bool, SectionNode> EnsureTrailingNewLine(Func<SectionNode, SectionNode> updateSection, IniDocument document)
        => (section, isLast)
            => isLast
                ? updateSection(section)
                : updateSection(section).EnsureTrailingNewLine(document.DetectNewLine());
}
