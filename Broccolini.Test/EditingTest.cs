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

        var updatedDocument = Parse(input).WithAppendedOrUpdatedSection("section", Identity);

        Assert.Equal(input, updatedDocument.ToString());
    }


    [Fact]
    public void AppendsSectionToEmptyDocument()
    {
        const string expected = "[section]";

        var updatedDocument = IniDocument.Empty.WithAppendedOrUpdatedSection("section", Identity);

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

        var updatedDocument = Parse(input.ReplaceLineEndings()).WithAppendedOrUpdatedSection("new section", Identity);

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
            .WithAppendedOrUpdatedSection("section", section => section.WithAppendedOrUpdatedEntry("key", "value"));

        Assert.Equal(expected.ReplaceLineEndings(), updatedDocument.ToString());
    }

    [Fact]
    public void AppendsKeyToExistingSection()
    {
        const string input =
            """
            [section]
            key = value
            """;
        const string expected =
            """
            [section]
            key = value
            new key = value
            """;

        var updatedDocument = Parse(input.ReplaceLineEndings())
            .WithAppendedOrUpdatedSection("section", section => section.WithAppendedOrUpdatedEntry("new key", "value"));

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
            .WithAppendedOrUpdatedSection("section", section => section.WithAppendedOrUpdatedEntry("key", "new value"));

        Assert.Equal(expected.ReplaceLineEndings(), updatedDocument.ToString());
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
            var parsedWithKey = Assert.IsType<KeyValueNode>(Assert.Single(Parse($"key = {input}").Children));
            Assert.Equal(expectedWithKey, parsedWithKey.WithValue(newValue).ToString());
        }

        [Fact]
        public void PreservesFormatting()
        {
            const string expected = "\tkey\t=\tnew value\t";
            const string input = "\tkey\t=\tvalue\t";
            var parsed = Assert.IsType<KeyValueNode>(Assert.Single(Parse(input).Children));
            Assert.Equal(expected, parsed.WithValue("new value").ToString());
        }
    }
}
