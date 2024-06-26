using Broccolini.Syntax;
using Xunit;
using static Broccolini.Test.TestData;
using static Broccolini.Tokenization.Tokenizer;

namespace Broccolini.Test;

public sealed class TokenizerTest
{
    [Theory]
    [MemberData(nameof(GetNewLinesData))]
    public void TokenizeNewLines(string input)
    {
        Assert.Single(Tokenize(input), new IniToken.NewLine(input));
    }

    private static TheoryData<string> GetNewLinesData() => NewLines.ToTheoryData();

    [Theory]
    [MemberData(nameof(GetConsecutiveNewLinesData))]
    public void TokenizesConsecutiveNewLines(params string[] inputs)
    {
        Assert.Equal(inputs.Select(x => new IniToken.NewLine(x)), Tokenize(inputs.ConcatToString()));
    }

    [Theory]
    [MemberData(nameof(GetWhiteSpaceData))]
    public void TokenizesWhiteSpace(char whitespace)
    {
        Assert.Single(Tokenize(whitespace.ToString()), new IniToken.WhiteSpace(whitespace.ToString()));
    }

    [Fact]
    public void CorrectlyTokenizesNewLineFollowingWhitesSpace()
    {
        Assert.Equal([new IniToken.WhiteSpace("\t"), new IniToken.NewLine("\n")], Tokenize("\t\n"));
    }

    private static TheoryData<char> GetWhiteSpaceData() => WhiteSpace.ToTheoryData();

    private static TheoryData<string, string> GetConsecutiveNewLinesData()
        => NewLines.SelectMany(_ => NewLines, ValueTuple.Create)
            .Except(Sequence.Return(("\r", "\n")))
            .ToTheoryData();
}
