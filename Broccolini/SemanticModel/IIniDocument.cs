namespace Broccolini.SemanticModel;

/// <remarks>Lookups ignore the key's case.</remarks>
public interface IIniDocument : IReadOnlyDictionary<string, IIniSection>
{
}
