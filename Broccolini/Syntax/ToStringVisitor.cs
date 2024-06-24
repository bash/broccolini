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

    public void Visit(KeyValueIniNode keyValueNode)
    {
        VisitTokens(keyValueNode.LeadingTrivia);
        _stringBuilder.Append(keyValueNode.Key);
        VisitToken(keyValueNode.TriviaBeforeEqualsSign);
        _stringBuilder.Append(keyValueNode.EqualsSign);
        VisitToken(keyValueNode.TriviaAfterEqualsSign);
        VisitToken(keyValueNode.Quote);
        _stringBuilder.Append(keyValueNode.Value);
        VisitToken(keyValueNode.Quote);
        VisitTokens(keyValueNode.TrailingTrivia);
        VisitToken(keyValueNode.NewLine);
    }

    public void Visit(UnrecognizedIniNode triviaNode)
    {
        VisitTokens(triviaNode.LeadingTrivia);
        VisitTokens(triviaNode.Tokens);
        VisitToken(triviaNode.NewLine);
        VisitTokens(triviaNode.TrailingTrivia);
    }

    public void Visit(SectionIniNode sectionNode)
    {
        VisitTokens(sectionNode.LeadingTrivia);
        Visit(sectionNode.Header);
        Visit(sectionNode.Children);
        VisitTokens(sectionNode.TrailingTrivia);
    }

    public void Visit(SectionHeaderIniNode sectionHeaderNode)
    {
        VisitTokens(sectionHeaderNode.LeadingTrivia);
        VisitToken(sectionHeaderNode.OpeningBracket);
        VisitToken(sectionHeaderNode.TriviaAfterOpeningBracket);
        _stringBuilder.Append(sectionHeaderNode.Name);
        VisitToken(sectionHeaderNode.TriviaBeforeClosingBracket);
        VisitToken(sectionHeaderNode.ClosingBracket);
        VisitTokens(sectionHeaderNode.UnrecognizedTokensAfterClosingBracket);
        VisitTokens(sectionHeaderNode.TrailingTrivia);
        VisitToken(sectionHeaderNode.NewLine);
    }

    public void Visit(CommentIniNode commentNode)
    {
        VisitTokens(commentNode.LeadingTrivia);
        VisitToken(commentNode.Semicolon);
        VisitToken(commentNode.TriviaAfterSemicolon);
        _stringBuilder.Append(commentNode.Text);
        VisitTokens(commentNode.TrailingTrivia);
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
