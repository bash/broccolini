using System.ComponentModel;
using System.Diagnostics;

namespace Broccolini.Syntax;

[EditorBrowsable(EditorBrowsableState.Advanced)]
[DebuggerDisplay("{ToString(),nq}")]
public abstract record IniToken
{
    private IniToken() { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected IniToken(IniToken original) { }

    private protected abstract void InternalImplementorsOnly();

    /// <summary>A new line (= ␊, ␍ or ␍␊). Use <see cref="IniSyntaxFactory.NewLine"/> to create this token.</summary>
    public sealed record NewLine : IniToken
    {
        internal NewLine(string value)
        {
            Value = value;
        }

        public string Value { get; init; }

        public override string ToString() => Value;

        private protected override void InternalImplementorsOnly() { }
    }

    /// <summary>One or more whitespace characters (= all ASCII characters ≤ <c>0x20</c> except ␍ and ␊).
    /// Use <see cref="IniSyntaxFactory.WhiteSpace"/> to create this token.</summary>
    public sealed record WhiteSpace : IniToken
    {
        internal WhiteSpace(string value)
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
        internal Identifier(string value)
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
