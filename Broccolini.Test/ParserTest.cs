using System.Diagnostics;
using Broccolini.Editing;
using Broccolini.Syntax;
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

    [Theory]
    [MemberData(nameof(LeadingTriviaData))]
    public void RecognizedLeadingWhitespaceAndNewLinesAsTrivia(IniNode node)
    {
        var expectedDocument = ToIniDocument(node);
        var parsedDocument = Parse(node.ToString());
        Assert.Equal(expectedDocument, parsedDocument);
    }

    public static TheoryData<IniNode> LeadingTriviaData()
       => (from node in ExampleNodes
           from breaking in LineBreakingTrivia
           from inline in InlineTrivia
           from inlineBeforeBreaking in InlineTrivia
           where (inlineBeforeBreaking.Length == 0) == (breaking.Length == 0)
           select ApplyLeadingTrivia(node.Value, inlineBeforeBreaking + breaking, inline)).ToTheoryData();

    private static IniNode ApplyLeadingTrivia(IniNode node, string trivia, string inlineTrivia)
        => node switch
        {
            SectionIniNode section => section with { LeadingTrivia = Tokenize(trivia), Header = section.Header with { LeadingTrivia = Tokenize(inlineTrivia) } },
            _ => node with { LeadingTrivia = Tokenize(trivia + inlineTrivia) },
        };

    [Theory]
    [MemberData(nameof(TrailingTriviaData))]
    public void RecognizedTrailingWhitespaceAndNewLinesAsTrivia(IniNode node)
    {
        var expectedDocument = ToIniDocument(node);
        var parsedDocument = Parse(expectedDocument.ToString());
        Assert.Equal(expectedDocument.ToString(), parsedDocument.ToString()); // Sanity check
        Assert.Equal(expectedDocument, parsedDocument);
    }

    private static TheoryData<IniNode> TrailingTriviaData()
       => (from node in ExampleNodes
           from inline in InlineTrivia
           from breaking in LineBreakingTrivia
           from inlineAfterBreaking in InlineTrivia
           where (inlineAfterBreaking.Length == 0) == (breaking.Length == 0)
           select ApplyTrailingTrivia(node.Value, inline, breaking + inlineAfterBreaking)).ToTheoryData();

    [Theory]
    [MemberData(nameof(TriviaForConsecutiveNodes))]
    public void RecognizedWhitespaceAndNewLinesAsTriviaForConsecutiveNodes(IniNode a, IniNode b)
    {
        var expectedDocument = Append(ToIniDocument(a), b);
        var parsedDocument = Parse(expectedDocument.ToString());
        Assert.Equal(expectedDocument.ToString(), parsedDocument.ToString()); // Sanity check
        Assert.Equal(expectedDocument, parsedDocument);
    }

    private static TheoryData<IniNode, IniNode> TriviaForConsecutiveNodes()
       => (from node1 in ExampleNodes
           from node2 in ExampleNodes
           from inline in InlineTrivia
           from breaking in LineBreakingTrivia
           from inlineLeading in InlineTrivia
           select (ApplyTrailingTrivia(node1.Value, inline, breaking), ApplyLeadingTrivia(node2.Value, "", inlineLeading))).ToTheoryData();

    private static IniNode ApplyTrailingTrivia(IniNode node, string inlineTrivia, string trivia)
        => node switch
        {
            SectionIniNode section => section with { TrailingTrivia = Tokenize(trivia), Header = section.Header with { TrailingTrivia = Tokenize(inlineTrivia) } },
            _ => node with { TrailingTrivia = Tokenize(inlineTrivia + trivia) },
        };

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
        => Append(IniDocument.Empty, node);

    private static IniDocument Append(IniDocument document, IniNode node)
    {
        document = document.EnsureTrailingNewLine(new IniToken.NewLine("\n"));
        return node switch
        {
            SectionIniNode section => document with { Sections = document.Sections.Add(section) },
            SectionChildIniNode child when document.Sections.Any() => AppendToLastSection(document, child),
            SectionChildIniNode child => document with { NodesOutsideSection = document.NodesOutsideSection.Add(child) },
            _ => throw new UnreachableException(),
        };
    }

    private static IniDocument AppendToLastSection(IniDocument document, SectionChildIniNode node)
    {
        var lastSection = document.Sections.Last();
        var updatedSection = lastSection with { Children = lastSection.Children.Add(node) };
        return document with { Sections = document.Sections.SetItem(document.Sections.Count - 1, updatedSection) };
    }
}
