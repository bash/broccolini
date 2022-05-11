namespace Broccolini.SemanticModel;

/// <remarks>Lookups ignore the key's case.</remarks>
public interface IDocument : IReadOnlyDictionary<string, ISection>
{
}
