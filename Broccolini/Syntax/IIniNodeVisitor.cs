using System.ComponentModel;

namespace Broccolini.Syntax;

[EditorBrowsable(EditorBrowsableState.Advanced)]
public interface IIniNodeVisitor
{
    void Visit(KeyValueIniNode keyValueNode);

    void Visit(UnrecognizedIniNode triviaNode);

    void Visit(SectionIniNode sectionNode);

    void Visit(CommentIniNode commentNode);
}
