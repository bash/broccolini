using System.Collections.Immutable;
using Broccolini.Syntax;

namespace Broccolini.SemanticModel;

public static class DocumentExtensions
{
    /// <summary>Converts the AST to a semantic representation of the INI document.</summary>
    /// <remarks>This representation is intended for reading only and discards formatting and trivia.</remarks>
    public static IDocument GetSemanticModel(this IniDocument document)
        => new Document(
            document.Sections
                .DistinctBy(section => section.Name)
                .ToImmutableDictionary(
                    section => section.Name,
                    section => (ISection)new Section(
                        section.Name,
                        section.Children
                            .OfType<KeyValueNode>()
                            .DistinctBy(kv => kv.Key)
                            .ToImmutableDictionary(kv => kv.Key, kv => kv.Value))));
}
