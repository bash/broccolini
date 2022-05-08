namespace Broccolini;

public abstract record Token
{
    public sealed record LineBreak(string Value) : Token
    {
        public override string ToString() => Value;
    }

    public sealed record WhiteSpace(string Value) : Token
    {
        public override string ToString() => Value;
    }

    public sealed record Comment(string Value) : Token
    {
        public override string ToString() => Value;
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

    public sealed record SingleQuotes : Token
    {
        public override string ToString() => "'";
    }

    public sealed record DoubleQuotes : Token
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
