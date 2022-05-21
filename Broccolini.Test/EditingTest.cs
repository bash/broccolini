using Xunit;
using Broccolini.Editing;
using Broccolini.Syntax;
using static Broccolini.IniParser;
using static Broccolini.Test.TestData;

namespace Broccolini.Test;

public sealed class EditingTest
{
    [Fact]
    public void DoesNothingWhenSectionExists()
    {
        const string input = "[section]";

        var updatedDocument = Parse(input).WithSection("section", Identity);

        Assert.Equal(input, updatedDocument.ToString());
    }


    [Fact]
    public void AppendsSectionToEmptyDocument()
    {
        const string expected = "[section]";

        var updatedDocument = IniDocument.Empty.WithSection("section", Identity);

        Assert.Equal(expected.ReplaceLineEndings(), updatedDocument.ToString());
    }

    [Theory]
    [MemberData(nameof(AppendSectionData))]
    public void AppendsSection(string expected, string input)
    {
        var updatedDocument = Parse(input.ReplaceLineEndings()).WithSection("new section", Identity);

        Assert.Equal(expected.ReplaceLineEndings(), updatedDocument.ToString());
    }

    public static TheoryData<string, string> AppendSectionData()
        => Sequence.Return(
            ($"[section]{Environment.NewLine}[new section]", "[section]"),
            ("[section]\n[new section]", "[section]\n"))
            .Concat(NewLines.Select(newLine => ($"; line 1{newLine};line 2{newLine}[new section]", $"; line 1{newLine};line 2")))
            .ToTheoryData();

    [Fact]
    public void AppendsKeyToNewSection()
    {
        const string expected =
            """
            [section]
            key = value
            """;

        var updatedDocument = IniDocument.Empty
            .WithSection("section", section => section.WithKeyValue("key", "value"));

        Assert.Equal(expected.ReplaceLineEndings(), updatedDocument.ToString());
    }

    public static TheoryData<string> NewLineData() => NewLines.ToTheoryData();

    [Theory]
    [MemberData(nameof(AppendsKeyToExistingSectionData))]
    public void AppendsKeyToExistingSection(string expected, string input)
    {
        var updatedDocument = Parse(input.ReplaceLineEndings())
            .WithSection("section", section => section.WithKeyValue("new key", "value"));

        Assert.Equal(expected.ReplaceLineEndings(), updatedDocument.ToString());
    }

    public static TheoryData<string, string> AppendsKeyToExistingSectionData()
        => Sequence.Return(
             ("""
             [section]
             new key = value
             """,
             """
             [section]

             """),
            ("""
             [section]
             key = value
             new key = value
             """,
             """
             [section]
             key = value
             """),
            (
             """
             [section]
             key = value
             new key = value
             [other section]
             """,
             """
             [section]
             key = value
             [other section]
             """
            ))
            .AsEnumerable()
            .SelectMany(_ => NewLines, (data, newLine) => (data.Item1.ReplaceLineEndings(newLine), data.Item2.ReplaceLineEndings(newLine)))
            .ToTheoryData();

    [Fact]
    public void ReplacesValueInExistingSection()
    {
        const string input =
            """
            [section]
            key   =   value
            """;
        const string expected =
            """
            [section]
            key   =   new value
            """;

        var updatedDocument = Parse(input.ReplaceLineEndings())
            .WithSection("section", section => section.WithKeyValue("key", "new value"));

        Assert.Equal(expected.ReplaceLineEndings(), updatedDocument.ToString());
    }

    [Theory]
    [InlineData(
        """
        [section]
        key2 = value

        """,
        """
        [section]
        key = value
        key2 = value
        key=value 2
        """)]
    [InlineData(
        """
        [section]
        [section]
        key = value 2
        """,
        """
        [section]
        key = value
        [section]
        key = value 2
        """)]
    [InlineData(
        """
        [section]

        """,
        """
        [section]
        key = value
        """)]
    public void RemovesKeyFromSection(string expected, string input)
    {
        var parsed = Parse(input);
        var edited = parsed.UpdateSection("section", section => section.RemoveKeyValue("key"));
        Assert.Equal(expected, edited.ToString());
    }

    [Theory]
    [InlineData(
        "",
        """
        [section]
        """)]
    [InlineData(
        "",
        """
        [section]
        [section]
        """)]
    [InlineData(
        "",
        """
        [section]
        key = value
        """)]
    [InlineData(
        """
        ; trailing trivia node
        trailing garbage
        """,
        """
        [section]
        ; leading trivia node
        leading garbage
        key = value
        ; trailing trivia node
        trailing garbage
        """)]
    [InlineData(
        """
        ; trailing trivia node 1
        ; trailing trivia node 2
        trailing garbage
        """,
        """
        [section]
        ; trailing trivia node 1
        ; trailing trivia node 2
        trailing garbage
        """)]
    [InlineData(
        """
        [leading section]
        ; trailing trivia node
        trailing garbage
        """,
        """
        [leading section]
        [section]
        leading garbage
        ; leading trivia node
        key = value
        ; trailing trivia node
        trailing garbage
        """)]
    [InlineData(
        """
        [leading section]
        ; trailing trivia node
        trailing garbage
        [other section]
        ; trailing trivia node
        trailing garbage
        """,
        """
        [leading section]
        [section]
        ; trailing trivia node
        trailing garbage
        [other section]
        [section]
        ; trailing trivia node
        trailing garbage
        """)]
    public void RemovesSection(string expected, string input)
    {
        var parsed = Parse(input);
        var edited = parsed.RemoveSection("section");
        Assert.Equal(expected, edited.ToString());
        Assert.Equal(edited, Parse(edited.ToString()));
    }

    public sealed class WithValue
    {
        [Theory]
        [InlineData("' value needs quotes '", "'value'", " value needs quotes ")]
        [InlineData("'no need for quotes'", "'value'", "no need for quotes")]
        [InlineData("\" value needs quotes \"", "\"value\"", " value needs quotes ")]
        [InlineData("\"no need for quotes\"", "\"value\"", "no need for quotes")]
        public void PreservesExistingQuotes(string expected, string input, string newValue)
        {
            var expectedWithKey = $"key = {expected}";
            var parsedWithKey = Assert.IsType<KeyValueNode>(Assert.Single(Parse($"key = {input}").NodesOutsideSection));
            Assert.Equal(expectedWithKey, parsedWithKey.WithValue(newValue).ToString());
        }

        [Fact]
        public void PreservesFormatting()
        {
            const string expected = "\tkey\t=\tnew value\t";
            const string input = "\tkey\t=\tvalue\t";
            var parsed = Assert.IsType<KeyValueNode>(Assert.Single(Parse(input).NodesOutsideSection));
            Assert.Equal(expected, parsed.WithValue("new value").ToString());
        }
    }

    public sealed class DetectNewLine
    {
        [Fact]
        public void UsesNativeNewLinesForEmptyDocument()
        {
            Assert.Equal(new Token.NewLine(Environment.NewLine), IniDocument.Empty.DetectNewLine());
        }

        [Theory]
        [MemberData(nameof(NewLineData))]
        public void UsesNewLineOfFirstNode(string newLine, string input)
        {
            Assert.Equal(new Token.NewLine(newLine), Parse(input).DetectNewLine());
        }

        public static TheoryData<string, string> NewLineData()
            => Sequence.Return(
                "garbage\n",
                "; comment\n",
                "[section]\n",
                "[section]\nkey = value")
                .SelectMany(_ => NewLines.AsEnumerable(), (input, newLine) => (newLine, input.ReplaceLineEndings(newLine)))
                .ToTheoryData();
    }
}
