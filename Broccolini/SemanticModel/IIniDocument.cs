namespace Broccolini.SemanticModel;

/// <summary><para>An immutable semantic view of an INI document.
/// A document is a collection of sections.</para>
/// <para>Use <see cref="IniDocumentExtensions.ToSemanticModel"/> to
/// convert a parse result to an <see cref="IIniDocument"/>.</para></summary>
/// <example><code>
/// [database]
/// server = "localhost"
/// port = 1234
/// [logging]
/// level = "verbose"
/// </code></example>
/// <remarks>Lookups ignore the key's case.</remarks>
public interface IIniDocument : IReadOnlyDictionary<string, IIniSection>
{
}
