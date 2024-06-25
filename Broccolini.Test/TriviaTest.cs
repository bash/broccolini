using System.Diagnostics;
using Broccolini.Editing;
using Broccolini.Syntax;
using FsCheck;
using Xunit;
using static Broccolini.IniParser;
using static Broccolini.Test.TestData;
using static Broccolini.Tokenization.Tokenizer;

namespace Broccolini.Test;

public sealed class TriviaTest
{
    [Fact]
    public void Example()
    {
        var input =
            """
            \t
            \t[section]\t
            \t
            \tkey=value\t
            \t\n
            """
            .Replace(@"\t", "\t")
            .Replace(@"\n", "\n")
            .ReplaceLineEndings("\n");

        var sectionHeader = new SectionHeaderIniNode("section")
        {
            LeadingTrivia = Tokenize("\t"),
            TrailingTrivia = Tokenize("\t\n\t"),
            NewLine = new IniToken.NewLine("\n"),
        };

        var keyValue = new KeyValueIniNode("key", "value")
        {
            LeadingTrivia = Tokenize("\t"),
            TrailingTrivia = Tokenize("\t"),
            NewLine = new IniToken.NewLine("\n"),
        };

        var section = new SectionIniNode(sectionHeader, [keyValue])
        {
            LeadingTrivia = Tokenize("\t\n"),
            TrailingTrivia = Tokenize("\t\n"),
        };

        var expectedDocument = IniDocument.Empty with
        {
            Sections = [section],
        };
        Assert.Equal(expectedDocument.ToString(), input); // Sanity check
        var parsed = Parse(input);
        Assert.Equal(expectedDocument, parsed);
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
