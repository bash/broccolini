using Broccolini.Editing;
using Broccolini.Syntax;
using Xunit;
using static Broccolini.IniParser;

namespace Broccolini.Test.Editing;

public sealed class TrailingNewLineTest
{
    [Fact]
    public void DoesNothingForEmptyDocument()
    {
        var newLine = new Token.LineBreak("\n");
        Assert.Equal(IniDocument.Empty, IniDocument.Empty.EnsureTrailingNewLine(newLine));
    }

    [Theory]
    [MemberData(nameof(NodesWithoutNewlines))]
    public void AddsTrailingNewLine(string input)
    {
        var newLine = new Token.LineBreak("\n");
        var document = Parse(input);
        Assert.Equal(input + "\n", document.EnsureTrailingNewLine(newLine).ToString());
    }

    [Theory]
    [MemberData(nameof(NodesWithoutNewlines))]
    public void DoesNothingWhenNewLineIsAlreadyPresent(string input)
    {
        var inputWithTrailingNewLine = input + "\n";
        var document = Parse(inputWithTrailingNewLine);
        Assert.Equal(inputWithTrailingNewLine, document.EnsureTrailingNewLine(new Token.LineBreak("\r\n")).ToString());
    }

    public static TheoryData<string> NodesWithoutNewlines()
        => new()
        {
            "key = value",
            "[empty section]",
            "[section]\nkey = value",
            "[section]\nkey1 = value\nkey2 = value",
            "; comment",
            "garbage",
            "[one]\n[two]\n[three]",
        };
}
