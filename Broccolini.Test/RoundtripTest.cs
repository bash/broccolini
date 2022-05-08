using FsCheck;
using FsCheck.Xunit;
using Xunit;
using static Broccolini.IniParser;
using static Broccolini.Test.TestData;

namespace Broccolini.Test;

public sealed class RoundtripTest
{
    [Fact]
    public void PreservesFormattingOfExampleFile()
    {
        var input = File.ReadAllText(Path.Combine("Resources", "roundtrip.ini"));
        var document = Parse(input);
        Assert.Equal(input, document.ToString());
    }

    [Theory]
    [MemberData(nameof(PreservesFormattingData))]
    public void PreservesFormatting(string input)
    {
        var document = Parse(input);
        Assert.Equal(input, document.ToString());
    }

    public static TheoryData<string> PreservesFormattingData()
        => Sequence.Concat(
            CommentNodes,
            GarbageNodes,
            LeadingNodes,
            LineBreaks,
            SectionsWithNames.Select(s => s.Input),
            KeyValuePairsWithKeyAndValue.Select(s => s.Input)).ToTheoryData();

    [Property]
    public Property PreservesFormattingOfArbitraryInput(NonNull<string> input)
    {
        var document = Parse(input.Get);
        return (input.Get == document.ToString()).ToProperty();
    }
}
