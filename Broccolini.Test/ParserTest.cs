using Broccolini.Syntax;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using static Broccolini.IniParser;
using static Broccolini.Test.TestData;

namespace Broccolini.Test;

public sealed class ParserTest
{
    public ParserTest()
    {
        BroccoliniGenerators.Register();
    }

    [Theory]
    [MemberData(nameof(GetCommentsData))]
    public void ParsesCommentNode(string input, string leadingNode)
    {
        var document = Parse(leadingNode + input);
        var node = Assert.IsType<CommentIniNode>(GetLastNode(document));
        Assert.Equal(input, node.ToString());
    }

    [Property]
    public Property ParsesArbitraryComment(string commentValue)
    {
        var input = $"; {commentValue}";
        var document = Parse(input);
        return (document.NodesOutsideSection is [CommentIniNode commentNode] && commentNode.ToString() == input)
            .ToProperty()
            .When(!input.Contains('\r') && !input.Contains('\n'));
    }

    public static TheoryData<string, string> GetCommentsData()
        => CommentNodes.Select(c => (c, string.Empty))
            .Concat(CommentNodes.SelectMany(_ => LeadingNodes, ValueTuple.Create))
            .ToTheoryData();

    [Theory]
    [MemberData(nameof(GetGarbageData))]
    public void ParsesGarbageAsUnrecognized(string input, string leadingNode)
    {
        var document = Parse(leadingNode + input);
        var node = Assert.IsType<UnrecognizedIniNode>(GetLastNode(document));
        Assert.Equal(input, node.ToString());
    }

    public static TheoryData<string, string> GetGarbageData()
        => GarbageNodes.Select(c => (c, string.Empty))
            .Concat(GarbageNodes.SelectMany(_ => LeadingNodes, ValueTuple.Create))
            .ToTheoryData();

    [Theory]
    [MemberData(nameof(GetSectionNameData))]
    public void ParsesSectionNames(string name, string input)
    {
        var document = Parse(input);
        var node = Assert.IsType<SectionIniNode>(document.Sections.Last());
        Assert.Equal(name, node.Name);
    }

    [Property]
    public bool ParsesArbitrarySectionName(SectionName name, Whitespace ws1, Whitespace ws2, Whitespace ws3, InlineText trailing)
    {
        var document = Parse($"{ws1.Value}[{ws2.Value}{name.Value}{ws3.Value}]{trailing.Value}");
        return document.Sections is [{ Name: var actualName }] && actualName == name.Value;
    }

    [Property]
    public bool ParsesArbitrarySectionNameWithoutClosingBracket(SectionName name, Whitespace ws1, Whitespace ws2, Whitespace ws3)
    {
        var document = Parse($"{ws1.Value}[{ws2.Value}{name.Value}{ws3.Value}");
        return document.Sections is [{ Name: var actualName }] && actualName == name.Value;
    }

    public static TheoryData<string, string> GetSectionNameData()
        => SectionsWithNames.Select(s => (s.Name, s.Input)).ToTheoryData();

    [Theory]
    [MemberData(nameof(GetKeyValuePairData))]
    public void ParsesKeyValuePair(string key, string value, string input)
    {
        var document = Parse(input);
        var node = Assert.IsType<KeyValueIniNode>(document.NodesOutsideSection.Last());
        Assert.Equal(key, node.Key);
        Assert.Equal(value, node.Value);
    }

    [Fact]
    public void ParsesNodesSeparatedByWhitespaceAndNewline()
    {
        var document = Parse("one=1\t\ntwo=2");
        Assert.Equal(2, document.NodesOutsideSection.Count);
        var secondNode = Assert.IsType<KeyValueIniNode>(document.NodesOutsideSection.Last());
        Assert.Equal("two", secondNode.Key);
        Assert.Equal("2", secondNode.Value);
    }

    public static TheoryData<string, string, string> GetKeyValuePairData()
        => KeyValuePairsWithKeyAndValue.Select(s => (s.Key, s.Value, s.Input)).ToTheoryData();

    private static IniNode GetLastNode(IniDocument document)
        => GetLastNode(document.GetNodes());

    private static IniNode GetLastNode(IEnumerable<IniNode> nodes)
        => nodes.Last() switch
        {
            SectionIniNode sectionNode => GetLastNode(sectionNode),
            var node => node,
        };

    private static IniNode GetLastNode(SectionIniNode sectionNode)
        => sectionNode.Children.Count > 0
            ? GetLastNode(sectionNode.Children)
            : sectionNode;
}
