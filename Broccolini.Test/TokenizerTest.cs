using Broccolini.Syntax;
using Xunit;
using static Broccolini.Test.TestData;
using static Broccolini.Tokenization.Tokenizer;

namespace Broccolini.Test;

public sealed class TokenizerTest
{
    [Theory]
    [MemberData(nameof(GetLineBreaksData))]
    public void TokenizesLineBreaks(string input)
    {
        Assert.Single(Tokenize(input), new Token.LineBreak(input));
    }

    private static TheoryData<string> GetLineBreaksData() => LineBreaks.ToTheoryData();

    [Theory]
    [MemberData(nameof(GetConsecutiveLineBreaksData))]
    public void TokenizesConsecutiveLineBreaks(params string[] inputs)
    {
        Assert.Equal(inputs.Select(x => new Token.LineBreak(x)), Tokenize(inputs.ConcatToString()));
    }

    private static TheoryData<string, string> GetConsecutiveLineBreaksData()
        => LineBreaks.SelectMany(_ => LineBreaks, ValueTuple.Create)
            .Except(Sequence.Return(("\r", "\n")))
            .ToTheoryData();
}
