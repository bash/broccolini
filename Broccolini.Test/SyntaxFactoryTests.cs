using Broccolini.Syntax;
using Broccolini.Tokenization;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using static Broccolini.Syntax.IniSyntaxFactory;
using static Broccolini.Test.TestData;

namespace Broccolini.Test;

public sealed class SyntaxFactoryTests
{
    public SyntaxFactoryTests()
    {
        BroccoliniGenerators.Register();
    }

    [Property]
    public Property ParsesCreatedSectionNodeOrThrows(NonNull<string> value)
    {
        try
        {
            var node = Section(value.Get);
            var parsedNode = IniParser.Parse(node.ToString());
            return (parsedNode.Sections.Count == 1
                    && parsedNode.Sections.Single() is SectionIniNode sectionNode
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
                && parsedNode.NodesOutsideSection.First() is KeyValueIniNode keyValueNode
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
        var parsedNode = Assert.IsType<KeyValueIniNode>(Assert.Single(IniParser.Parse(node.ToString()).NodesOutsideSection));
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
            return (parsedNode.NodesOutsideSection.Count == 1 && parsedNode.NodesOutsideSection.Single() is KeyValueIniNode keyValueNode
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

    [Theory]
    [MemberData(nameof(NewLinesData))]
    public void ThrowsWhenNewLineIsUsedAsWhiteSpace(string newLine)
    {
        Assert.Throws<ArgumentException>(() => WhiteSpace(newLine));
    }

    [Property]
    public Property TokenizerAndFactoryAcceptSameWhiteSpace(WhitespaceNoNulls whitespace)
    {
        var tokenFromFactory = Option<IniToken>.None;
        try
        {
            tokenFromFactory = WhiteSpace(whitespace.Value);
        }
        catch (ArgumentException)
        {
        }

        var tokenized = Tokenizer.Tokenize(whitespace.Value);
        return (tokenized.SingleOrNone() == tokenFromFactory).ToProperty();
    }

    private static TheoryData<string> NewLinesData() => NewLines.ToTheoryData();

    private static TheoryData<string> WhiteSpaceData() => TestData.WhiteSpace.Select(static c => c.ToString()).ToTheoryData();
}
