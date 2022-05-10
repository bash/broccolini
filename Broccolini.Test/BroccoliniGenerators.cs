using FsCheck;

namespace Broccolini.Test;

public static class BroccoliniGenerators
{
    public static void Register()
    {
        Arb.Register(typeof(BroccoliniGenerators));
    }

    public static Arbitrary<ValidSectionName> GenerateValidSectionName()
        => Arb.From<string>()
            .Filter(value => value is not null)
            .Filter(value => !value.Contains('\n') && !value.Contains('\r') && !value.Contains(']') && value.Trim(' ', '\f', '\v', '\t') == value)
            .Convert(value => new ValidSectionName(value), validSectionName => validSectionName.Value);

    public static Arbitrary<ValidKey> GenerateValidKey()
        => Arb.From(
            GenerateStringFromMeaningfulChars()
                .Where(value => value is not null)
                .Where(value => !value.Contains('\n') && !value.Contains('\r') && value.Trim(' ', '\f', '\v', '\t') == value && !value.StartsWith('[') && !value.Contains('='))
                .Select(value => new ValidKey(value)));

    public static Arbitrary<ValidValue> GenerateValidValue()
        => Arb.From(
            GenerateStringFromMeaningfulChars()
                .Where(value => value is not null)
                .Where(value => !value.Contains('\n') && !value.Contains('\r'))
                .Select(value => new ValidValue(value)));

    private static Gen<string> GenerateStringFromMeaningfulChars()
        => from length in Arb.Generate<PositiveInt>()
           from chars in Gen.Sequence(Enumerable.Range(0, length.Get).Select(_ => Gen.OneOf(Sequence.Return('\r', '\n', ';', ' ', '\t', '\v', '\f', '[', ']', '=', '\"', '\'', 'a', 'b', 'c').Select(Gen.Constant))))
           select chars.ConcatToString();
}

public sealed record ValidSectionName(string Value)
{
    public override string ToString() => Value;
}

public sealed record ValidKey(string Value)
{
    public override string ToString() => Value;
}

public sealed record ValidValue(string Value)
{
    public override string ToString() => Value;
}
