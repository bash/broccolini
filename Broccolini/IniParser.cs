using System.Diagnostics.Contracts;
using Broccolini.Parsing;
using Broccolini.SemanticModel;
using Broccolini.Syntax;
using Broccolini.Tokenization;

namespace Broccolini;

public static class IniParser
{
    /// <summary><para>Parses an INI document to an AST. This function always succeeds even when invalid input is provided.</para>
    /// <para>Use <see cref="ParseToSemanticModel"/> if you want to get a semantic representation instead.
    /// You can alternatively use <see cref="SemanticModel.IniDocumentExtensions.ToSemanticModel"/> on the parsed AST document.</para></summary>
    /// <example>
    /// Editing a document
    /// <code>
    /// var document = IniParser.Parse(File.ReadAllText("config.ini"));
    /// var updated = document
    ///     .WithSection("owner", section => section.WithKeyValue("name", "John Doe"))
    ///     .UpdateSection("database", section => section.RemoveKeyValue("port"));
    /// File.WriteAllText("config.ini", updated.ToString(), Encoding.Unicode);
    /// </code>
    /// </example>
    [Pure]
    public static IniDocument Parse(string input)
    {
        var tokens = Tokenizer.Tokenize(input);
        return Parser.Parse(new ParserInput(tokens));
    }

    /// <summary><para>Parses an INI document to a semantic representation.
    /// This function always succeeds even when invalid input is provided.</para>
    /// <para>Use <see cref="Parse"/> if you want to get an editable AST instead.</para></summary>
    /// <example>
    /// Reading values from a document
    /// <code>
    /// var document = IniParser.ParseToSemanticModel(File.ReadAllText("config.ini"));
    /// string databaseServer = document["database"]["server"];
    /// string databasePort = document["database"]["port"];
    /// </code>
    /// </example>
    [Pure]
    public static IIniDocument ParseToSemanticModel(string input)
        => Parse(input).ToSemanticModel();
}
