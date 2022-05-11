using Broccolini.Parsing;
using Broccolini.Syntax;
using Broccolini.Tokenization;

namespace Broccolini;

public static class IniParser
{
    /// <summary><para>Parses an INI document to an AST. This function always succeeds even when invalid input is provided.</para>
    /// <para>Use <see cref="SemanticModel.DocumentExtensions.GetSemanticModel(IniDocument)"/> to get a semantic view of the document
    /// or one of the methods on <see cref="Editing.EditingExtensions"/> to edit the document.</para></summary>
    public static IniDocument Parse(string input)
    {
        var tokens = Tokenizer.Tokenize(input);
        return Parser.Parse(new ParserInput(tokens));
    }
}
