namespace Broccolini.Syntax;

public abstract record Token
{
    private Token() { }

    public sealed record NewLine : Token
    {
        public NewLine(string value)
        {
            Value = value;
        }

        public string Value { get; init; }

        public override string ToString() => Value;
    }

    public sealed record WhiteSpace : Token
    {
        public WhiteSpace(string value)
        {
            Value = value;
        }

        public string Value { get; init; }

        public override string ToString() => Value;
    }

    public sealed record Semicolon : Token
    {
        public override string ToString() => ";";
    }

    public sealed record OpeningBracket : Token
    {
        public override string ToString() => "[";
    }

    public sealed record ClosingBracket : Token
    {
        public override string ToString() => "]";
    }

    public sealed record EqualsSign : Token
    {
        public override string ToString() => "=";
    }

    public abstract record Quote : Token
    {
        internal Quote() { }
    }

    public sealed record SingleQuote : Quote
    {
        public override string ToString() => "'";
    }

    public sealed record DoubleQuote : Quote
    {
        public override string ToString() => "\"";
    }

    public sealed record Identifier : Token
    {
        public Identifier(string value)
        {
            Value = value;
        }

        public string Value { get; init; }

        public override string ToString() => Value;
    }

    internal sealed record Epsilon : Token
    {
        public override string ToString() => string.Empty;
    }
}
