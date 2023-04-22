using Broccolini.Syntax;

namespace Broccolini.Editing;

internal static class NewlineDetectionExtensions
{
    private static readonly IniToken.NewLine NativeNewLine = new(Environment.NewLine);

    public static IniToken.NewLine DetectNewLine(this IniDocument document)
        => document.GetNodes()
            .Select(n => n.NewLine)
            .FirstOrDefault()
                ?? NativeNewLine;

    public static IniToken.NewLine DetectNewLine(this SectionIniNode node)
        => node.NewLine ?? node.NewLineHint ?? NativeNewLine;
}
