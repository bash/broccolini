namespace Broccolini.SemanticModel;

public interface ISection : IReadOnlyDictionary<string, string>
{
    string Name { get; }
}
