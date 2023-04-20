namespace Broccolini.SemanticModel;

/// <remarks>Lookups ignore the key's case.</remarks>
public interface IIniSection : IReadOnlyDictionary<string, string>
{
    string Name { get; }
}
