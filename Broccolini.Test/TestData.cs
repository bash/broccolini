using Broccolini.Syntax;
using Broccolini.Tokenization;

namespace Broccolini.Test;

internal static class TestData
{
    // This tests against a bug in the tokenizer that classifies whitespace + newline as whitespace.
    private const string NewLineWithPrecedingWhiteSpace = "\t\n";

    // TODO: test parsing with newline at the end
    public static IEnumerable<string> GarbageNodes
        => Sequence.Return(
            "garbage",
            "    garbage with leading whitespace",
            "garbage with trailing whitespace     ");

    // TODO: test parsing with newline at the end
    public static IEnumerable<string> CommentNodes
        => Sequence.Return(
            "; this is a comment",
            "    ; comment with leading whitespace",
            "; comment with trailing whitespace      ",
            "; key = value",
            "; [section]");

    public static IEnumerable<string> LeadingNodes
        => Sequence.Return(
            "; comment\r\n" +
            "garbage\r\n",
            "[section]\r\n",
            "key = value\r\n");

    public static IEnumerable<string> InlineTrivia
        => ["\t", "    ", ""];

    public static IEnumerable<string> LineBreakingTrivia
        => ContextFreeNewLines
            .Concat(ContextFreeNewLines.SelectMany(_ => InlineTrivia, (nl, inline) => $"{nl}{inline}{nl}"))
            .Append("");

    public static IEnumerable<string> LeadingNodesOrTrivia
        => LeadingNodes.Concat(InlineTrivia.SelectMany(_ => LineBreakingTrivia, (inline, breaking) => inline + breaking));

    public static IEnumerable<SectionWithName> SectionsWithNames
        => Sequence.Return(
            new SectionWithName("[", string.Empty),
            new SectionWithName("[section", "section"),
            new SectionWithName("[Brötli", "Brötli"),
            new SectionWithName("[section =", "section ="),
            new SectionWithName("[section with \t\v\f whitespace inside", "section with \t\v\f whitespace inside"),
            new SectionWithName("[[[[[section", "[[[[section"),
            new SectionWithName("[\"section\"]", "\"section\""),
            new SectionWithName("['section']", "'section'"))
            .SelectMany(VaryLeadingWhitespace)
            .SelectMany(VaryTrailingWhitespace)
            .SelectMany(VaryClosingBracket)
            .SelectMany(VaryTrailingWhitespace)
            .SelectMany(VaryNewLines)
            .SelectMany(VaryLeadingNewLines)
            .Distinct();

    public static IEnumerable<KeyValuePairWithKeyAndValue> KeyValuePairsWithKeyAndValue
        => Sequence.Return(
            new KeyValuePairWithKeyAndValue("=", string.Empty, string.Empty),
            new KeyValuePairWithKeyAndValue("=\" \"", string.Empty, " "),
            new KeyValuePairWithKeyAndValue("=\"\u0011\"", string.Empty, "\u0011"),
            new KeyValuePairWithKeyAndValue(" = value", string.Empty, "value"),
            new KeyValuePairWithKeyAndValue("key =", "key", string.Empty),
            new KeyValuePairWithKeyAndValue("key=value", "key", "value"),
            new KeyValuePairWithKeyAndValue("  key  =  value  ", "key", "value"),
            new KeyValuePairWithKeyAndValue("key] = value", "key]", "value"),
            new KeyValuePairWithKeyAndValue("key = ; not a comment", "key", "; not a comment"),
            new KeyValuePairWithKeyAndValue("key with space = value", "key with space", "value"),
            new KeyValuePairWithKeyAndValue("key = value with space", "key", "value with space"),
            new KeyValuePairWithKeyAndValue("key ; not a comment = value", "key ; not a comment", "value"),
            new KeyValuePairWithKeyAndValue("key = \\n", "key", "\\n"),
            new KeyValuePairWithKeyAndValue("key\\=key = value", "key\\", "key = value"),
            new KeyValuePairWithKeyAndValue("# key = not a comment", "# key", "not a comment"),
            new KeyValuePairWithKeyAndValue("key = \"quoted value'", "key", "\"quoted value'"),
            new KeyValuePairWithKeyAndValue("key = 'quoted value\"", "key", "'quoted value\""))
            .Concat(KeyValuePairsWithQuotes)
            .SelectMany(VaryLeadingNewLines);
            // TODO: vary leading and trailing whitespace and line break

    public static IEnumerable<string> NewLines => ["\r\n", "\r", "\n"];

    public static IEnumerable<string> ContextFreeNewLines => ["\r\n", "\n"];

    public static IEnumerable<CaseSensitivityInput> CaseSensitivityInputs
        => Sequence.Return(
            new CaseSensitivityInput("foo", "FOO", ShouldBeEqual: true),
            new CaseSensitivityInput("Brötchen", "BRÖTCHEN", ShouldBeEqual: true),
            new CaseSensitivityInput("español", "ESPAÑOL", ShouldBeEqual: true),
            new CaseSensitivityInput("é", "É", ShouldBeEqual: true),
            new CaseSensitivityInput("ß", "ẞ", ShouldBeEqual: false),
            new CaseSensitivityInput("İ", "i", ShouldBeEqual: false),
            new CaseSensitivityInput("ı", "I", ShouldBeEqual: false))
                .SelectMany(pair => Sequence.Return(pair, new CaseSensitivityInput(pair.Variant2, pair.Variant1, ShouldBeEqual: pair.ShouldBeEqual)));

    public static IEnumerable<char> WhiteSpace
        => Enumerable.Range(1, 0x20)
            .Skip(1) // U+0000 is not whitespace
            .Select(n => (char)n)
            .Except(Sequence.Return('\r', '\n'));

    public static IEnumerable<ExampleNode> ExampleNodes
        => [
            new UnrecognizedIniNode(Tokenizer.Tokenize("garbage")) { NewLine = new IniToken.NewLine("\n") },
            new CommentIniNode("comment") { NewLine = new IniToken.NewLine("\n") },
            new KeyValueIniNode("key", "value") { NewLine = new IniToken.NewLine("\n") },
            new SectionIniNode(new SectionHeaderIniNode("section") { NewLine = new IniToken.NewLine("\n") }, []),
            new SectionIniNode(new SectionHeaderIniNode("section") { NewLine = new IniToken.NewLine("\n") }, [new KeyValueIniNode("child-key", "value") { NewLine = new IniToken.NewLine("\n") }]),
           ];

    private static IEnumerable<KeyValuePairWithKeyAndValue> KeyValuePairsWithQuotes
        => Sequence.Return(
            new KeyValuePairWithKeyAndValue("\"quoted key\" = \"quoted value\"", "\"quoted key\"", "quoted value"),
            new KeyValuePairWithKeyAndValue("key = \"quoted value\"", "key", "quoted value"),
            new KeyValuePairWithKeyAndValue("key = \"left quote only", "key", "\"left quote only"),
            new KeyValuePairWithKeyAndValue("key = right quote only\"", "key", "right quote only\""),
            new KeyValuePairWithKeyAndValue("key = quotes \" within \"", "key", "quotes \" within \""),
            new KeyValuePairWithKeyAndValue("key = \" quotes \" within", "key", "\" quotes \" within"),
            new KeyValuePairWithKeyAndValue("key = quotes \" \" within", "key", "quotes \" \" within"),
            new KeyValuePairWithKeyAndValue("key = \"unbalanced right\"\"\"", "key", "unbalanced right\"\""),
            new KeyValuePairWithKeyAndValue("key = \"\"\"unbalanced left\"", "key", "\"\"unbalanced left"),
            new KeyValuePairWithKeyAndValue("key = \"\"\"balanced\"\"\"", "key", "\"\"balanced\"\""),
            new KeyValuePairWithKeyAndValue("key = \\\"escaped quotes\\\"", "key", "\\\"escaped quotes\\\""),
            new KeyValuePairWithKeyAndValue("key =   \"    whitespace in quotes    \"  ", "key", "    whitespace in quotes    "))
            .SelectMany(VaryQuotes);

    private static IEnumerable<KeyValuePairWithKeyAndValue> VaryQuotes(KeyValuePairWithKeyAndValue keyValuePair)
        => Sequence.Return(
            keyValuePair,
            new KeyValuePairWithKeyAndValue(
                keyValuePair.Input.Replace('"', '\''),
                keyValuePair.Key.Replace('"', '\''),
                keyValuePair.Value.Replace('"', '\'')));

    private static IEnumerable<SectionWithName> VaryClosingBracket(SectionWithName sectionWithName)
        => Sequence.Return(
            sectionWithName,
            sectionWithName with { Input = sectionWithName.Input + ']' },
            sectionWithName with { Input = sectionWithName.Input + "] garbage garbage" },
            sectionWithName with { Input = sectionWithName.Input + "]]]]]]" });

    private static IEnumerable<SectionWithName> VaryLeadingWhitespace(SectionWithName sectionWithName)
        => Sequence.Return(
            sectionWithName,
            sectionWithName with { Input = " \t\v\f " + sectionWithName.Input });

    private static IEnumerable<SectionWithName> VaryLeadingNewLines(SectionWithName sectionWithName)
        => [sectionWithName,
            sectionWithName with { Input = "\n" + sectionWithName.Input },
            sectionWithName with { Input = "; comment" + NewLineWithPrecedingWhiteSpace + sectionWithName.Input }];

    private static IEnumerable<KeyValuePairWithKeyAndValue> VaryLeadingNewLines(KeyValuePairWithKeyAndValue keyValuePair)
        => [keyValuePair,
            keyValuePair with { Input = "\n" + keyValuePair.Input },
            keyValuePair with { Input = "; comment" + NewLineWithPrecedingWhiteSpace + keyValuePair.Input }];

    private static IEnumerable<SectionWithName> VaryTrailingWhitespace(SectionWithName sectionWithName)
        => Sequence.Return(
            sectionWithName,
            sectionWithName with { Input = sectionWithName.Input + " \t\v\f " });

    private static IEnumerable<SectionWithName> VaryNewLines(SectionWithName sectionWithName)
        => Sequence.Return(
            sectionWithName,
            sectionWithName with { Input = sectionWithName.Input + "\r\n" });
}

public sealed record SectionWithName(string Input, string Name);

public sealed record KeyValuePairWithKeyAndValue(string Input, string Key, string Value);

public sealed record CaseSensitivityInput(string Variant1, string Variant2, bool ShouldBeEqual);

public sealed record ExampleNode(IniNode Value)
{
    public static implicit operator ExampleNode(IniNode node) => new(node);
}
