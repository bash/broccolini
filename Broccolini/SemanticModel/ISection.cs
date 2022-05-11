namespace Broccolini.SemanticModel;

/// <remarks>Lookups ignore the key's case.</remarks>
public interface ISection : IReadOnlyDictionary<string, string>
{
    string Name { get; }
}
