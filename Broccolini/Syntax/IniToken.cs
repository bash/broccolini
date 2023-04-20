using System.ComponentModel;

namespace Broccolini.Syntax;

public abstract record IniToken
{
    private IniToken() { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected IniToken(IniToken original) { }

    private protected abstract void InternalImplementorsOnly();

    public sealed record NewLine : IniToken
    {
        public NewLine(string value)
        {
            Value = value;
        }

        public string Value { get; init; }

        public override string ToString() => Value;

        private protected override void InternalImplementorsOnly() { }
    }

    public sealed record WhiteSpace : IniToken
    {
        public WhiteSpace(string value)
        {
            Value = value;
        }

        public string Value { get; init; }

        public override string ToString() => Value;

        private protected override void InternalImplementorsOnly() { }
    }

    public sealed record Semicolon : IniToken
    {
        public override string ToString() => ";";

        private protected override void InternalImplementorsOnly() { }
    }

    public sealed record OpeningBracket : IniToken
    {
        public override string ToString() => "[";

        private protected override void InternalImplementorsOnly() { }
    }

    public sealed record ClosingBracket : IniToken
    {
        public override string ToString() => "]";

        private protected override void InternalImplementorsOnly() { }
    }

    public sealed record EqualsSign : IniToken
    {
        public override string ToString() => "=";

        private protected override void InternalImplementorsOnly() { }
    }

    public abstract record Quote : IniToken
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

    public sealed record Identifier : IniToken
    {
        public Identifier(string value)
        {
            Value = value;
        }

        public string Value { get; init; }

        public override string ToString() => Value;

        private protected override void InternalImplementorsOnly() { }
    }

    internal sealed record Epsilon : IniToken
    {
        public override string ToString() => string.Empty;

        private protected override void InternalImplementorsOnly() { }
    }
}
