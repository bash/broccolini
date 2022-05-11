using Broccolini.Syntax;
using System.Diagnostics.Contracts;
using static Broccolini.Syntax.SyntaxFactory;

namespace Broccolini.Editing;

public static partial class EditingExtensions
{
    /// <summary>Appends or updates a key-value entry.</summary>
    [Pure]
    public static SectionNode WithKeyValue(this SectionNode sectionNode, string key, string value)
        => sectionNode.Children.TryUpdateFirst(
            node => node is KeyValueNode { Key: var k } && k == key,
            node => ((KeyValueNode)node).WithValue(value),
            out var updatedChildren)
                ? sectionNode with { Children = updatedChildren }
                : sectionNode.AppendChild(KeyValue(key, value));

    /// <summary>Updates a key-value entry. Does nothing when the entry does not exist.</summary>
    [Pure]
    public static SectionNode UpdateKeyValue(this SectionNode sectionNode, string key, string value)
         => sectionNode.Children.TryUpdateFirst(
            node => node is KeyValueNode { Key: var k } && k == key,
            node => ((KeyValueNode)node).WithValue(value),
            out var updatedChildren)
                ? sectionNode with { Children = updatedChildren }
                : sectionNode;

    /// <summary>Removes all entries from the section with the given key.</summary>
    [Pure]
    public static SectionNode RemoveKeyValue(this SectionNode sectionNode, string key) => throw new NotImplementedException();

    private static SectionNode AppendChild(this SectionNode sectionNode, SectionChildNode node)
    {
        var sectionWithNewLine = sectionNode.EnsureTrailingNewLine(DefaultNewLine);
        return sectionWithNewLine with { Children = sectionWithNewLine.Children.Add(node) };
    }
}
