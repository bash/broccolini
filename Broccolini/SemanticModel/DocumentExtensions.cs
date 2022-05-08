using Broccolini.Syntax;

namespace Broccolini.SemanticModel;

public static class DocumentExtensions
{
    /// <summary>Converts the AST to a semantic representation of the INI document.</summary>
    /// <remarks>This representation is intended for reading only and discards formatting and trivia.</remarks>
    public static IDocument GetSemanticModel(this IniDocument document) => throw new NotImplementedException();
}
