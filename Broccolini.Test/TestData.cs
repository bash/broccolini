using System.Collections.Generic;
using System.Linq;
using Funcky;

namespace Broccolini.Test;

internal static class TestData
{
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
            "; comment with trailing whitespace      ");

    public static IEnumerable<string> LeadingNodes
        => Sequence.Return(
            "; comment\r\n" +
            "garbage\r\n",
            "[section]\r\n",
            "\r\n",
            "key = value\r\n");

    public static IEnumerable<SectionWithName> SectionsWithNames
        => Sequence.Return(
            new SectionWithName("[", string.Empty),
            new SectionWithName("[section", "section"),
            new SectionWithName("[section =", "section ="),
            new SectionWithName("[section with \t\v\f whitespace inside", "section with \t\v\f whitespace inside"),
            new SectionWithName("[[[[[section", "[[[[section"),
            new SectionWithName("[\"section\"]", "\"section\""),
            new SectionWithName("['section']", "'section'"))
            .SelectMany(VaryLeadingWhitespace)
            .SelectMany(VaryTrailingWhitespace)
            .SelectMany(VaryClosingBracket)
            .SelectMany(VaryTrailingWhitespace)
            .SelectMany(VaryLineBreak)
            .Distinct();

    public static IEnumerable<KeyValuePairWithKeyAndValue> KeyValuePairsWithKeyAndValue
        => Sequence.Return(
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
            .Concat(KeyValuePairsWithQuotes);
            // TODO: vary leading and trailing whitespace and line break

    public static IEnumerable<string> LineBreaks => Sequence.Return("\r\n", "\r", "\n");

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

    private static IEnumerable<SectionWithName> VaryTrailingWhitespace(SectionWithName sectionWithName)
        => Sequence.Return(
            sectionWithName,
            sectionWithName with { Input = sectionWithName.Input + " \t\v\f " });

    private static IEnumerable<SectionWithName> VaryLineBreak(SectionWithName sectionWithName)
        => Sequence.Return(
            sectionWithName,
            sectionWithName with { Input = sectionWithName.Input + "\r\n" });
}

public sealed record SectionWithName(string Input, string Name);

public sealed record KeyValuePairWithKeyAndValue(string Input, string Key, string Value);
