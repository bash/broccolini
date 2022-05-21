namespace Broccolini.Syntax;

public interface IIniNodeVisitor
{
    void Visit(KeyValueNode keyValueNode);

    void Visit(UnrecognizedNode triviaNode);

    void Visit(SectionNode sectionNode);

    void Visit(CommentNode commentNode);
}
