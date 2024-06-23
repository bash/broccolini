using System.Text;

namespace Broccolini.Syntax;

internal sealed class ToStringVisitor : IIniNodeVisitor
{
    private readonly StringBuilder _stringBuilder = new();

    public override string ToString() => _stringBuilder.ToString();

    public void Visit(IEnumerable<IniNode> nodes)
    {
        foreach (var node in nodes)
        {
            node.Accept(this);
        }
    }

    public void Visit(IniSectionHeader sectionHeader)
    {
        VisitToken(sectionHeader.LeadingTrivia);
        _stringBuilder.Append(sectionHeader.OpeningBracket);
        VisitToken(sectionHeader.TriviaAfterOpeningBracket);
        _stringBuilder.Append(sectionHeader.Name);
        VisitToken(sectionHeader.TriviaBeforeClosingBracket);
        VisitToken(sectionHeader.ClosingBracket);
        VisitTokens(sectionHeader.TrailingTrivia);
    }

    public void Visit(KeyValueIniNode keyValueNode)
    {
        VisitToken(keyValueNode.LeadingTrivia);
        _stringBuilder.Append(keyValueNode.Key);
        VisitToken(keyValueNode.TriviaBeforeEqualsSign);
        _stringBuilder.Append(keyValueNode.EqualsSign);
        VisitToken(keyValueNode.TriviaAfterEqualsSign);
        VisitToken(keyValueNode.Quote);
        _stringBuilder.Append(keyValueNode.Value);
        VisitToken(keyValueNode.Quote);
        VisitToken(keyValueNode.TrailingTrivia);
        VisitToken(keyValueNode.NewLine);
    }

    public void Visit(UnrecognizedIniNode triviaNode)
    {
        VisitTokens(triviaNode.Tokens);
        VisitToken(triviaNode.NewLine);
    }

    public void Visit(SectionIniNode sectionNode)
    {
        Visit(sectionNode.Header);
        VisitToken(sectionNode.NewLine);
        Visit(sectionNode.Children);
    }

    public void Visit(CommentIniNode commentNode)
    {
        VisitToken(commentNode.LeadingTrivia);
        VisitToken(commentNode.Semicolon);
        VisitToken(commentNode.TriviaAfterSemicolon);
        _stringBuilder.Append(commentNode.Text);
        VisitToken(commentNode.TrailingTrivia);
        VisitToken(commentNode.NewLine);
    }

    private void VisitTokens(IEnumerable<IniToken> tokens)
    {
        foreach (var token in tokens)
        {
            _stringBuilder.Append(token);
        }
    }

    private void VisitToken(IniToken? token)
    {
        if (token is not null)
        {
            _stringBuilder.Append(token);
        }
    }
}
