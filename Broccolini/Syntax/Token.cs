namespace Broccolini.Syntax;

public abstract record Token
{
    private Token() { }

    public sealed record NewLine(string Value) : Token
    {
        public override string ToString() => Value;
    }

    public sealed record WhiteSpace(string Value) : Token
    {
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

    public sealed record Identifier(string Value) : Token
    {
        public override string ToString() => Value;
    }

    public sealed record Epsilon : Token
    {
        public override string ToString() => string.Empty;
    }
}
