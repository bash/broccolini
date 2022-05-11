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

    public void Visit(KeyValueNode keyValueNode)
    {
        VisitTrivia(keyValueNode.LeadingTrivia);
        _stringBuilder.Append(keyValueNode.Key);
        VisitTrivia(keyValueNode.TriviaBeforeEqualsSign);
        _stringBuilder.Append(keyValueNode.EqualsSign);
        VisitTrivia(keyValueNode.TriviaAfterEqualsSign);
        VisitToken(keyValueNode.OpeningQuote);
        _stringBuilder.Append(keyValueNode.Value);
        VisitToken(keyValueNode.ClosingQuote);
        VisitTrivia(keyValueNode.TrailingTrivia);
        VisitToken(keyValueNode.NewLine);
    }

    public void Visit(TriviaNode triviaNode)
    {
        VisitTrivia(triviaNode.Value);
        VisitToken(triviaNode.NewLine);
    }

    public void Visit(SectionNode sectionNode)
    {
        VisitTrivia(sectionNode.LeadingTrivia);
        _stringBuilder.Append(sectionNode.OpeningBracket);
        VisitTrivia(sectionNode.TriviaAfterOpeningBracket);
        _stringBuilder.Append(sectionNode.Name);
        VisitTrivia(sectionNode.TriviaBeforeClosingBracket);
        VisitToken(sectionNode.ClosingBracket);
        VisitTrivia(sectionNode.TrailingTrivia);
        VisitToken(sectionNode.NewLine);
        Visit(sectionNode.Children);
    }

    private void VisitTrivia(TriviaList trivia)
    {
        foreach (var token in trivia.Tokens)
        {
            _stringBuilder.Append(token);
        }
    }

    private void VisitToken(Token? token)
    {
        if (token is not null)
        {
            _stringBuilder.Append(token);
        }
    }
}
