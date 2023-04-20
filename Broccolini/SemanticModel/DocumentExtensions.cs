using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using Broccolini.Syntax;
using static Broccolini.SemanticModel.KeyComparision;

namespace Broccolini.SemanticModel;

public static class IniDocumentExtensions
{
    /// <summary>Converts the AST to a semantic representation of the INI document.</summary>
    /// <remarks>This representation is intended for reading only and discards formatting and trivia.</remarks>
    [Pure]
    public static IDocument GetSemanticModel(this IniDocument document)
        => new Document(
            document.Sections
                .DistinctBy(section => section.Name, KeyComparer)
                .ToImmutableDictionary(
                    section => section.Name,
                    section => (ISection)new Section(
                        section.Name,
                        section.Children
                            .OfType<KeyValueNode>()
                            .DistinctBy(kv => kv.Key, KeyComparer)
                            .ToImmutableDictionary(kv => kv.Key, kv => kv.Value, keyComparer: KeyComparer)),
                    keyComparer: KeyComparer));
}
