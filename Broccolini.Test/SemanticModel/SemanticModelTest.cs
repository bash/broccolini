using Broccolini.SemanticModel;
using Xunit;
using static Broccolini.Test.SemanticModel.ReferenceImplementation;

namespace Broccolini.Test.SemanticModel;

public sealed class SemanticModelTest
{
    [Fact]
    public void ReturnsEmptyDocumentWhenInputIsEmpty()
    {
        var (reference, document) = Parse(string.Empty);

        Assert.Empty(document);

        AssertEquals(reference, document);
    }

    [Fact]
    public void ReturnsEmptySectionWhenSectionExistsButIsEmpty()
    {
        var (reference, document) = Parse("[section]");

        var section = Assert.Single(document);
        Assert.Equal("section", section.Key);
        Assert.Equal("section", section.Value.Name);
        Assert.Empty(section.Value);

        AssertEquals(reference, document);
    }

    [Fact]
    public void ReturnSectionWithKeys()
    {
        var (reference, document) = Parse("[section]\r\nkey = value\r\nkey2 = value2");

        var section = Assert.Single(document);
        Assert.Equal("section", section.Key);
        Assert.Equal("section", section.Value.Name);
        Assert.Equal(Sequence.Return(KeyValuePair.Create("key", "value"), KeyValuePair.Create<string, string>("key2", "value2")), section.Value.OrderBy(kvp => kvp.Key));

        AssertEquals(reference, document);
    }

    [Fact]
    public void ReturnsValueOfFirstOccurrenceOfDuplicatedKeys()
    {
        var (reference, document) = Parse("[section]\r\nkey = value\r\nkey = value2");

        Assert.Equal("value", document["section"]["key"]);

        AssertEquals(reference, document);
    }

    [Fact]
    public void IgnoresValuesFromSecondOccurrenceOfSection()
    {
        var (reference, document) = Parse("[section]first = 1\r\n[section]\r\nsecond = 2");

        Assert.DoesNotContain("second", document["section"]);

        AssertEquals(reference, document);
    }

    [Fact]
    public void IgnoresLinesWithNoEqualsSign()
    {
        var (reference, document) = Parse("[section]\r\nkey");

        Assert.DoesNotContain("key", document["section"]);

        AssertEquals(reference, document);
    }

    [Fact]
    public void ReturnsSectionWithEmptyName()
    {
        var (reference, document) = Parse("[");

        Assert.Contains(string.Empty, document);

        AssertEquals(reference, document);
    }

    private static (IDocument?, IDocument) Parse(string input)
    {
        var document = IniParser.Parse(input).GetSemanticModel();
        return OperatingSystem.IsWindows()
            ? (CreateReferenceDocument(input), document)
            : (null, document);
    }

    private static void AssertEquals(IDocument? expected, IDocument actual)
    {
        if (expected is null) return;

        Assert.Equal(expected.Keys.OrderBy(Identity), actual.Keys.OrderBy(Identity));

        foreach (var (expectedSection, actualSection) in expected.Values.OrderBy(s => s.Name).Zip(actual.Values.OrderBy(s => s.Name)))
        {
            Assert.Equal(expectedSection.Count, actualSection.Count);
            Assert.Equal(expectedSection.OrderBy(pair => pair.Key), actualSection.OrderBy(pair => pair.Key));
        }
    }
}

