namespace Broccolini.SemanticModel;

public interface IDocument
{
    IReadOnlyDictionary<string, string> TopLevelKeys { get; }

    IReadOnlyDictionary<string, ISection> Sections { get; }
}
