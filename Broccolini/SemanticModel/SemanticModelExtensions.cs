using System.Diagnostics.Contracts;
using Broccolini.Syntax;
using Broccolini.SemanticModel;
using static Broccolini.SemanticModel.KeyComparision;

namespace Broccolini;

public static class SemanticModelExtensions
{
    /// <summary>Converts the AST to a semantic representation of the INI document.</summary>
    /// <remarks>This representation is intended for reading only and discards formatting and trivia.</remarks>
    [Pure]
    public static IIniDocument ToSemanticModel(this IniDocument document)
        => new Document(
            document.Sections
                .DistinctBy(section => section.Name, KeyComparer)
                .ToDictionary(
                    section => section.Name,
                    section => (IIniSection)new Section(
                        section.Name,
                        section.Children
                            .OfType<KeyValueIniNode>()
                            .DistinctBy(kv => kv.Key, KeyComparer)
                            .ToDictionary(kv => kv.Key, kv => kv.Value, comparer: KeyComparer)),
                    comparer: KeyComparer));
}
