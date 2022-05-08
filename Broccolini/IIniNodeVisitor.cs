namespace Broccolini;

public interface IIniNodeVisitor
{
    void Visit(KeyValueNode keyValueNode);

    void Visit(TriviaNode triviaNode);

    void Visit(SectionNode sectionNode);
}
