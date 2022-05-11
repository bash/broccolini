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
        Assert.Single(Tokenize(input), new Token.NewLine(input));
    }

    private static TheoryData<string> GetNewLinesData() => NewLines.ToTheoryData();

    [Theory]
    [MemberData(nameof(GetConsecutiveNewLinesData))]
    public void TokenizesConsecutiveNewLines(params string[] inputs)
    {
        Assert.Equal(inputs.Select(x => new Token.NewLine(x)), Tokenize(inputs.ConcatToString()));
    }

    private static TheoryData<string, string> GetConsecutiveNewLinesData()
        => NewLines.SelectMany(_ => NewLines, ValueTuple.Create)
            .Except(Sequence.Return(("\r", "\n")))
            .ToTheoryData();
}
