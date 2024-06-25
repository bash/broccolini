using Broccolini.Syntax;
using System.Diagnostics.Contracts;
using Broccolini.Editing;
using static Broccolini.SemanticModel.KeyComparision;
using static Broccolini.Syntax.IniSyntaxFactory;

// ReSharper disable once CheckNamespace
namespace Broccolini;

public static partial class EditingExtensions
{
    /// <summary>Appends or updates a key-value entry.</summary>
    [Pure]
    public static SectionIniNode WithKeyValue(this SectionIniNode sectionNode, string key, string value)
        => sectionNode.Children.TryUpdateFirst(
            node => node is KeyValueIniNode { Key: var k } && KeyEquals(k, key),
            node => ((KeyValueIniNode)node).WithValue(value),
            out var updatedChildren)
                ? sectionNode with { Children = updatedChildren }
                : sectionNode.AppendChild(KeyValue(key, value));

    /// <summary>Updates a key-value entry. Does nothing when the entry does not exist.</summary>
    [Pure]
    public static SectionIniNode UpdateKeyValue(this SectionIniNode sectionNode, string key, string value)
         => sectionNode.Children.TryUpdateFirst(
            node => node is KeyValueIniNode { Key: var k } && KeyEquals(k, key),
            node => ((KeyValueIniNode)node).WithValue(value),
            out var updatedChildren)
                ? sectionNode with { Children = updatedChildren }
                : sectionNode;

    /// <summary>Removes all entries from the section with the given key.</summary>
    [Pure]
    public static SectionIniNode RemoveKeyValue(this SectionIniNode sectionNode, string key)
        => sectionNode with { Children = sectionNode.Children.RemoveAll(node => node is KeyValueIniNode { Key: var k } && KeyEquals(k, key)) };

    private static SectionIniNode AppendChild(this SectionIniNode sectionNode, SectionChildIniNode node)
    {
        var sectionWithNewLine = sectionNode.EnsureTrailingNewLine(sectionNode.DetectNewLine());
        return sectionWithNewLine with { Children = sectionWithNewLine.Children.Add(node) };
    }
}
