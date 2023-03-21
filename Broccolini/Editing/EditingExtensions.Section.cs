using Broccolini.Syntax;
using System.Diagnostics.Contracts;
using static Broccolini.SemanticModel.KeyComparision;
using static Broccolini.Syntax.SyntaxFactory;

namespace Broccolini.Editing;

public static partial class EditingExtensions
{
    /// <summary>Appends or updates a key-value entry.</summary>
    [Pure]
    public static SectionNode WithKeyValue(this SectionNode sectionNode, string key, string value)
        => sectionNode.Children.TryUpdateFirst(
            node => node is KeyValueNode { Key: var k } && KeyEquals(k, key),
            node => ((KeyValueNode)node).WithValue(value),
            out var updatedChildren)
                ? sectionNode with { Children = updatedChildren }
                : sectionNode.AppendChild(KeyValue(key, value));

    /// <summary>Updates a key-value entry. Does nothing when the entry does not exist.</summary>
    [Pure]
    public static SectionNode UpdateKeyValue(this SectionNode sectionNode, string key, string value)
         => sectionNode.Children.TryUpdateFirst(
            node => node is KeyValueNode { Key: var k } && KeyEquals(k, key),
            node => ((KeyValueNode)node).WithValue(value),
            out var updatedChildren)
                ? sectionNode with { Children = updatedChildren }
                : sectionNode;

    /// <summary>Removes all entries from the section with the given key.</summary>
    [Pure]
    public static SectionNode RemoveKeyValue(this SectionNode sectionNode, string key)
        => sectionNode with { Children = sectionNode.Children.RemoveAll(node => node is KeyValueNode { Key: var k } && KeyEquals(k, key)) };

    private static SectionNode AppendChild(this SectionNode sectionNode, SectionChildNode node)
    {
        return sectionNode.Children.TryFindIndex(IsBlank, out var index)
            ? InsertAtIndex(index)
            : AppendTrailing();

        SectionNode InsertAtIndex(int childIndex)
            => sectionNode with { Children = sectionNode.Children.Insert(childIndex, node.EnsureTrailingNewLine(sectionNode.DetectNewLine())) };

        SectionNode AppendTrailing()
        {
            var sectionWithNewLine = sectionNode.EnsureTrailingNewLine(sectionNode.DetectNewLine());
            return sectionWithNewLine with { Children = sectionWithNewLine.Children.Add(node) };
        }

        static bool IsBlank(SectionChildNode node)
            => node is UnrecognizedNode unrecognizedNode && unrecognizedNode.IsBlank();
    }
}
