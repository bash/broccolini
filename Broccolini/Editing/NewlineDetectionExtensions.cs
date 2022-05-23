using Broccolini.Syntax;

namespace Broccolini.Editing;

internal static class NewlineDetectionExtensions
{
    private static readonly Token.NewLine NativeNewLine = new(Environment.NewLine);

    public static Token.NewLine DetectNewLine(this IniDocument document)
        => document.GetNodes()
            .Select(n => n.NewLine)
            .FirstOrDefault()
                ?? NativeNewLine;

    public static Token.NewLine DetectNewLine(this SectionNode node)
        => node.NewLine ?? node.NewLineHint ?? NativeNewLine;
}
