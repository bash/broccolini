using System.Diagnostics.Contracts;
using Broccolini.Syntax;
using static Broccolini.Syntax.SyntaxFactory;

namespace Broccolini.Editing;

public static class EditingExtensions
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
    public static IniDocument UpdateSection(this IniDocument document, string sectionName, Func<SectionNode, SectionNode> updateSection) => throw new NotImplementedException();

    /// <summary>Removes all sections with the given name. Preserves trailing trivia.</summary>
    [Pure]
    public static IniDocument RemoveSection(this IniDocument document, string sectionName) => throw new NotImplementedException();

    /// <summary>Updates a key-value entry. Does nothing when the entry does not exist.</summary>
    [Pure]
    public static SectionNode UpdateKeyValue(this SectionNode sectionNode, string key, string value) => throw new NotImplementedException();

    /// <summary>Appends or updates a key-value entry.</summary>
    [Pure]
    public static SectionNode WithKeyValue(this SectionNode sectionNode, string key, string value)
        => sectionNode.Children.TryUpdateFirst(
            node => node is KeyValueNode { Key: var k } && k == key,
            node => ((KeyValueNode)node).WithValue(value),
            out var updatedChildren)
            ? sectionNode with { Children = updatedChildren }
            : sectionNode.AppendChild(KeyValue(key, value));

    /// <summary>Updates the value of a key-value node.</summary>
    /// <param name="value">The value may contain anything except newlines. Quotes are automatically added as needed to preserve whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when the updated node would result in something different when parsed back.</exception>
    [Pure]
    public static KeyValueNode WithValue(this KeyValueNode keyValueNode, string value)
    {
        var nodeWithNewValue = KeyValue(keyValueNode.Key, value);
        return keyValueNode with
        {
            Value = nodeWithNewValue.Value,
            OpeningQuote = keyValueNode.OpeningQuote ?? nodeWithNewValue.OpeningQuote,
            ClosingQuote = keyValueNode.OpeningQuote ?? nodeWithNewValue.ClosingQuote,
        };
    }

    /// <summary>Removes all entry from the section with the given key.</summary>
    [Pure]
    public static SectionNode RemoveKeyValue(this SectionNode sectionNode, string key) => throw new NotImplementedException();

    private static IniDocument AppendChild(this IniDocument document, IniNode node)
    {
        var documentWithNewLine = document.EnsureTrailingNewLine(DefaultNewLine);
        return documentWithNewLine with { Children = documentWithNewLine.Children.Add(node) };
    }

    private static SectionNode AppendChild(this SectionNode sectionNode, SectionChildNode node)
    {
        var sectionWithNewLine = sectionNode.EnsureTrailingNewLine(DefaultNewLine);
        return sectionWithNewLine with { Children = sectionWithNewLine.Children.Add(node) };
    }
}
