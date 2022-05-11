using Broccolini.Syntax;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using static Broccolini.Syntax.SyntaxFactory;
using static Broccolini.Test.TestData;

namespace Broccolini.Test;

public sealed class SyntaxFactoryTests
{
    [Property]
    public Property ParsesCreatedSectionNodeOrThrows(NonNull<string> value)
    {
        try
        {
            var node = Section(value.Get);
            var parsedNode = IniParser.Parse(node.ToString());
            return (parsedNode.Sections.Count == 1
                    && parsedNode.Sections.Single() is SectionNode sectionNode
                    && sectionNode.Name == value.Get).ToProperty();
        }
        catch (ArgumentException)
        {
            return true.ToProperty();
        }
    }

    [Theory]
    [InlineData("foo]")]
    [InlineData(" foo ")]
    [InlineData("\tfoo")]
    [InlineData("\vfoo")]
    [InlineData("\ffoo")]
    [MemberData(nameof(NewLinesData))]
    public void ThrowsWhenSectionNameWouldNotBePreserved(string value)
    {
        Assert.Throws<ArgumentException>(() => Section(value));
    }

    [Property]
    public Property ParsesCreatedKeyValueNodeOrThrows(NonNull<string> key, NonNull<string> value)
    {
        try
        {
            var node = KeyValue(key.Get, value.Get);
            var parsedNode = IniParser.Parse(node.ToString());
            return (parsedNode.NodesOutsideSection.Count == 1
                && parsedNode.NodesOutsideSection.First() is KeyValueNode keyValueNode
                && keyValueNode.Key == key.Get
                && keyValueNode.Value == value.Get).ToProperty();
        }
        catch (ArgumentException)
        {
            return true.ToProperty();
        }
    }

    [Theory]
    [InlineData("key", "value ")]
    [InlineData("key", " value")]
    [InlineData("key", " value ")]
    [InlineData("key", "\"value\"")]
    [InlineData("key", "\"value\" ")]
    [InlineData("key", " \"value\"")]
    [InlineData("key", " \"value\" ")]
    [InlineData("key", "'value'")]
    [InlineData("key", "'value' ")]
    [InlineData("key", " 'value'")]
    [InlineData("key", " 'value' ")]
    [InlineData("key", "    ")]
    public void ParsesCreatedKeyValueNode(string key, string value)
    {
        var node = KeyValue(key, value);
        var parsedNode = Assert.IsType<KeyValueNode>(Assert.Single(IniParser.Parse(node.ToString()).NodesOutsideSection));
        Assert.Equal(key, parsedNode.Key);
        Assert.Equal(value, parsedNode.Value);
    }

    [Property]
    public Property ParsesCreatedKeyValueNodeWithEmptyKeyOrThrows(NonNull<string> value)
    {
        try
        {
            var node = KeyValue(string.Empty, value.Get);
            var parsedNode = IniParser.Parse(node.ToString());
            return (parsedNode.NodesOutsideSection.Count == 1 && parsedNode.NodesOutsideSection.Single() is KeyValueNode keyValueNode
                && keyValueNode.Key == string.Empty
                && keyValueNode.Value == value.Get).ToProperty();
        }
        catch (ArgumentException)
        {
            return true.ToProperty();
        }
    }

    [Theory]
    [InlineData("[key")]
    [InlineData(" [key")]
    [InlineData("key =")]
    [InlineData("; comment")]
    [InlineData(" ; comment")]
    [MemberData(nameof(NewLinesData))]
    public void ThrowsWhenKeyIsInvalid(string key)
    {
        Assert.Throws<ArgumentException>(() => KeyValue(key, "value"));
    }

    [Theory]
    [MemberData(nameof(NewLinesData))]
    public void ThrowsWhenValueIsInvalid(string value)
    {
        Assert.Throws<ArgumentException>(() => KeyValue("key", value));
    }

    private static TheoryData<string> NewLinesData() => NewLines.ToTheoryData();
}
