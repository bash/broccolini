using System.Diagnostics;
using Broccolini.Syntax;
using Broccolini.Tokenization;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using static Broccolini.IniParser;
using static Broccolini.Test.TestData;
using static Broccolini.Tokenization.Tokenizer;

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
        return (document.NodesOutsideSection.Count == 1
                && document.NodesOutsideSection.First() is CommentIniNode triviaNode
                && triviaNode.ToString() == input)
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
        return document.Sections.Count == 1
            && document.Sections[0].Name == name.Value;
    }

    [Property]
    public bool ParsesArbitrarySectionNameWithoutClosingBracket(SectionName name, Whitespace ws1, Whitespace ws2, Whitespace ws3)
    {
        var document = Parse($"{ws1.Value}[{ws2.Value}{name.Value}{ws3.Value}");
        return (document.Sections.Count == 1
            && document.Sections[0].Name == name.Value);
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

    [Theory]
    [MemberData(nameof(LeadingTriviaData))]
    public void RecognizedLeadingWhitespaceAndNewLinesAsTrivia(ExampleNode node, string trivia, string inlineTrivia)
    {
        var expectedDocument = ToIniDocument(ApplyLeadingTrivia(node.Value, trivia, inlineTrivia));
        var parsedDocument = Parse($"{trivia}{inlineTrivia}{node.Value}");
        Assert.Equal(expectedDocument, parsedDocument);
    }

    private static IniNode ApplyLeadingTrivia(IniNode node, string trivia, string inlineTrivia)
        => node switch
        {
            SectionIniNode section => section with { LeadingTrivia = Tokenize(trivia), Header = section.Header with { LeadingTrivia = TokenizeWhiteSpace(inlineTrivia) } },
            _ => node with { LeadingTrivia = Tokenize(trivia + inlineTrivia) },
        };

    private static IniToken.WhiteSpace? TokenizeWhiteSpace(string input)
        => Tokenize(input) switch
        {
            [] => null,
            [IniToken.WhiteSpace ws] => ws,
            _ => throw new ArgumentException($"'{input}' is not valid whitespace", nameof(input)),
        };

    private static TheoryData<ExampleNode, string, string> LeadingTriviaData()
       => (from n in ExampleNodes
           from t in LeadingTrivia
           select (n, t.Item1, t.Item2)).ToTheoryData();

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

     private static IniDocument ToIniDocument(IniNode node)
        => node switch
        {
            SectionChildIniNode sectionChildNode => IniDocument.Empty with { NodesOutsideSection = [sectionChildNode] },
            SectionIniNode sectionNode => IniDocument.Empty with { Sections = [sectionNode] },
            _ => throw new UnreachableException(),
        };
}
