using Xunit;
using Broccolini.Editing;
using Broccolini.Syntax;
using static Broccolini.IniParser;

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
    [InlineData("[section]")]
    [InlineData("[section]\n")]
    public void AppendsSection(string input)
    {
        const string expected =
            """
            [section]
            [new section]
            """;

        var updatedDocument = Parse(input.ReplaceLineEndings()).WithSection("new section", Identity);

        Assert.Equal(expected.ReplaceLineEndings(), updatedDocument.ToString());
    }

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

    [Theory]
    [InlineData(
         """
         [section]
         key = value
         new key = value
         """,
         """
         [section]
         key = value
         """)]
    [InlineData(
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
         """)]
    public void AppendsKeyToExistingSection(string expected, string input)
    {
        var updatedDocument = Parse(input.ReplaceLineEndings())
            .WithSection("section", section => section.WithKeyValue("new key", "value"));

        Assert.Equal(expected.ReplaceLineEndings(), updatedDocument.ToString());
    }

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
        """,
        """
        [section]
        ; leading trivia node
        key = value
        ; trailing trivia node
        """)]
    [InlineData(
        """
        ; trailing trivia node 1
        ; trailing trivia node 2
        """,
        """
        [section]
        ; trailing trivia node 1
        ; trailing trivia node 2
        """)]
    [InlineData(
        """
        [leading section]
        ; trailing trivia node
        """,
        """
        [leading section]
        [section]
        ; leading trivia node
        key = value
        ; trailing trivia node
        """)]
    [InlineData(
        """
        [leading section]
        ; trailing trivia node
        [other section]
        ; trailing trivia node
        """,
        """
        [leading section]
        [section]
        ; trailing trivia node
        [other section]
        [section]
        ; trailing trivia node
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
}
