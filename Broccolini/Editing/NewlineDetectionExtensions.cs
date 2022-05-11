using Broccolini.Syntax;

namespace Broccolini.Editing;

internal static class NewlineDetectionExtensions
{
    private static readonly Token.LineBreak NativeNewLine = new(Environment.NewLine);

    public static Token DetectNewLine(this IniDocument document)
        => document.GetNodes()
            .Select(n => n.LineBreak)
            .FirstOrDefault()
                ?? NativeNewLine;

    public static Token DetectNewLine(this IniNode node)
        => node.LineBreak ?? NativeNewLine;
}
