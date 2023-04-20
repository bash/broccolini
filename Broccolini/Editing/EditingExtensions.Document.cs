using System.Diagnostics.Contracts;
using Broccolini.Syntax;
using static Broccolini.SemanticModel.KeyComparision;
using static Broccolini.Syntax.IniSyntaxFactory;

namespace Broccolini.Editing;

public static partial class EditingExtensions
{
    /// <summary>Appends or updates a section with the given name.</summary>
    [Pure]
    public static IniDocument WithSection(this IniDocument document, string name, Func<SectionIniNode, SectionIniNode> updateSection)
    {
        updateSection = WithNewLineHint(updateSection, document);
        return document.Sections.TryUpdateFirst(
            section => KeyEquals(section.Name, name),
            EnsureTrailingNewLine(updateSection, document),
            out var updatedSections)
                ? document with { Sections = updatedSections }
                : document.AppendSection(updateSection(Section(name)));
    }

    /// <summary>Updates a section with the given name. Does nothing when the section does not exist.</summary>
    [Pure]
    public static IniDocument UpdateSection(this IniDocument document, string name, Func<SectionIniNode, SectionIniNode> updateSection)
    {
        updateSection = WithNewLineHint(updateSection, document);
        return document.Sections.TryUpdateFirst(
            section => KeyEquals(section.Name, name),
            EnsureTrailingNewLine(updateSection, document),
            out var updatedSections)
                ? document with { Sections = updatedSections }
                : document;
    }

    /// <summary>Removes all sections with the given name. Preserves trailing trivia.</summary>
    [Pure]
    public static IniDocument RemoveSection(this IniDocument document, string name)
    {
        while (document.Sections.TryFindIndex(section => KeyEquals(section.Name, name), out var index))
        {
            var trailingTrivia = GetTrailingTrivia(document.Sections[index]);
            document = document with { Sections = document.Sections.RemoveAt(index) };
            document = InsertNodesAt(document, index, trailingTrivia);
        }

        return document;

        static IEnumerable<SectionChildIniNode> GetTrailingTrivia(SectionIniNode sectionNode)
            => sectionNode.Children.Reverse().TakeWhile(n => n is UnrecognizedIniNode or CommentIniNode).Reverse();

        static IniDocument InsertNodesAt(IniDocument document, int index, IEnumerable<SectionChildIniNode> nodesToInsert)
            => index >= 1 && document.Sections[index - 1] is var previousSection
                ? document with { Sections = document.Sections.SetItem(index - 1, previousSection with { Children = previousSection.Children.AddRange(nodesToInsert) }) }
                : document with { NodesOutsideSection = document.NodesOutsideSection.InsertRange(index, nodesToInsert) };
    }

    private static IniDocument AppendSection(this IniDocument document, SectionIniNode node)
    {
        var documentWithNewLine = document.EnsureTrailingNewLine(document.DetectNewLine());
        return documentWithNewLine with { Sections = documentWithNewLine.Sections.Add(node) };
    }

    private static Func<SectionIniNode, SectionIniNode> WithNewLineHint(Func<SectionIniNode, SectionIniNode> updateSection, IniDocument document)
        => section => updateSection(section with { NewLineHint = document.DetectNewLine() }) with { NewLineHint = null };

    private static Func<SectionIniNode, bool, SectionIniNode> EnsureTrailingNewLine(Func<SectionIniNode, SectionIniNode> updateSection, IniDocument document)
        => (section, isLast)
            => isLast
                ? updateSection(section)
                : updateSection(section).EnsureTrailingNewLine(document.DetectNewLine());
}
