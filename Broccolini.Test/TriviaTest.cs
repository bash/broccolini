using Broccolini.Syntax;
using Xunit;
using static Broccolini.IniParser;
using static Broccolini.Tokenization.Tokenizer;

namespace Broccolini.Test;

public sealed class TriviaTest
{
    [Fact]
    public void Example()
    {
        var input =
            """
            \t
            \t[section]\t
            \t
            \tkey=value\t
            \t\n
            """
            .Replace(@"\t", "\t")
            .Replace(@"\n", "\n")
            .ReplaceLineEndings("\n");

        var sectionHeader = new SectionHeaderIniNode("section")
        {
            LeadingTrivia = Tokenize("\t"),
            TrailingTrivia = Tokenize("\t\n\t"),
            NewLine = new IniToken.NewLine("\n"),
        };

        var keyValue = new KeyValueIniNode("key", "value")
        {
            LeadingTrivia = Tokenize("\t"),
            TrailingTrivia = Tokenize("\t"),
            NewLine = new IniToken.NewLine("\n"),
        };

        var section = new SectionIniNode(sectionHeader, [keyValue])
        {
            LeadingTrivia = Tokenize("\t\n"),
            TrailingTrivia = Tokenize("\t\n"),
        };

        var expectedDocument = IniDocument.Empty with
        {
            Sections = [section],
        };
        Assert.Equal(expectedDocument.ToString(), input); // Sanity check
        var parsed = Parse(input);
        Assert.Equal(expectedDocument, parsed);
    }
}
