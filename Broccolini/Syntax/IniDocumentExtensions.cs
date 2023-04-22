namespace Broccolini.Syntax;

internal static class IniDocumentExtensions
{
    public static IEnumerable<IniNode> GetNodes(this IniDocument document)
        => ((IEnumerable<IniNode>)document.NodesOutsideSection).Concat(document.Sections);
}
