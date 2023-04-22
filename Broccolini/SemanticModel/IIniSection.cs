namespace Broccolini.SemanticModel;

/// <summary>A section of an <see cref="IIniDocument"/>. A section is a collection of key-value pairs.</summary>
/// <example><code>
/// [database]
/// server = "localhost"
/// port = 1234
/// </code></example>
/// <remarks>Lookups ignore the key's case.</remarks>
public interface IIniSection : IReadOnlyDictionary<string, string>
{
    string Name { get; }
}
