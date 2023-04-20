using System.ComponentModel;

namespace Broccolini.Syntax;

public abstract record Token
{
    private Token() { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected Token(Token original) { }

    private protected abstract void InternalImplementorsOnly();

    public sealed record NewLine : Token
    {
        public NewLine(string value)
        {
            Value = value;
        }

        public string Value { get; init; }

        public override string ToString() => Value;

        private protected override void InternalImplementorsOnly() { }
    }

    public sealed record WhiteSpace : Token
    {
        public WhiteSpace(string value)
        {
            Value = value;
        }

        public string Value { get; init; }

        public override string ToString() => Value;

        private protected override void InternalImplementorsOnly() { }
    }

    public sealed record Semicolon : Token
    {
        public override string ToString() => ";";

        private protected override void InternalImplementorsOnly() { }
    }

    public sealed record OpeningBracket : Token
    {
        public override string ToString() => "[";

        private protected override void InternalImplementorsOnly() { }
    }

    public sealed record ClosingBracket : Token
    {
        public override string ToString() => "]";

        private protected override void InternalImplementorsOnly() { }
    }

    public sealed record EqualsSign : Token
    {
        public override string ToString() => "=";

        private protected override void InternalImplementorsOnly() { }
    }

    public abstract record Quote : Token
    {
        private protected Quote() { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected Quote(Quote original) : base(original) { }
    }

    public sealed record SingleQuote : Quote
    {
        public override string ToString() => "'";

        private protected override void InternalImplementorsOnly() { }
    }

    public sealed record DoubleQuote : Quote
    {
        public override string ToString() => "\"";

        private protected override void InternalImplementorsOnly() { }
    }

    public sealed record Identifier : Token
    {
        public Identifier(string value)
        {
            Value = value;
        }

        public string Value { get; init; }

        public override string ToString() => Value;

        private protected override void InternalImplementorsOnly() { }
    }

    internal sealed record Epsilon : Token
    {
        public override string ToString() => string.Empty;

        private protected override void InternalImplementorsOnly() { }
    }
}
