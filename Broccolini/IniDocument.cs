using System.Collections.Immutable;
using System.Diagnostics;

namespace Broccolini;

public sealed record IniDocument(IImmutableList<IniNode> Children)
{
    public override string ToString()
    {
        var visitor = new ToStringVisitor();
        visitor.Visit(Children);
        return visitor.ToString();
    }
}

public abstract record IniNode
{
    public Option<Token> LineBreak { get; init; }

    public abstract void Accept(IIniNodeVisitor visitor);

    public override string ToString()
    {
        var visitor = new ToStringVisitor();
        Accept(visitor);
        return visitor.ToString();
    }
}

public abstract record KeyValueOrTriviaNode : IniNode
{
    public override string ToString() => base.ToString();
}

/// <summary>A key-value pair: <c>key = value</c>.</summary>
[DebuggerDisplay("{Key}{EqualsSign}{Value}")]
public sealed record KeyValueNode(string Key, string Value) : KeyValueOrTriviaNode
{
    /// <summary>Leading whitespace</summary>
    public TriviaList LeadingTrivia { get; init; } = TriviaList.Empty;

    public TriviaList TriviaBeforeEqualsSign { get; init; } = TriviaList.Empty;

    public Token EqualsSign { get; init; } = new Token.EqualsSign();

    public TriviaList TriviaAfterEqualsSign { get; init; } = TriviaList.Empty;

    public Option<Token> OpeningQuote { get; init; }

    public Option<Token> ClosingQuote { get; init; }

    /// <summary>Trailing whitespace and line breaks.</summary>
    public TriviaList TrailingTrivia { get; init; } = TriviaList.Empty;

    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();
}

/// <summary>A comment or an unrecognized line.</summary>
[DebuggerDisplay("{Value}")]
public sealed record TriviaNode(TriviaList Value) : KeyValueOrTriviaNode
{
    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();
}

/// <summary>A section:
/// <code>[section]
/// key = value</code></summary>
[DebuggerDisplay("{OpeningBracket}{Name}{ClosingBracketDebugView}")]
public sealed record SectionNode(string Name, IImmutableList<KeyValueOrTriviaNode> Children) : IniNode
{
    /// <summary>Leading whitespace</summary>
    public TriviaList LeadingTrivia { get; init; } = TriviaList.Empty;

    public Token OpeningBracket { get; init; } = new Token.OpeningBracket();

    public TriviaList TriviaAfterOpeningBracket { get; init; } = TriviaList.Empty;

    public TriviaList TriviaBeforeClosingBracket { get; init; } = TriviaList.Empty;

    public Option<Token> ClosingBracket { get; init; } = new Token.ClosingBracket();

    /// <summary>Trailing whitespace, line breaks and garbage after the closing bracket.</summary>
    public TriviaList TrailingTrivia { get; init; } = TriviaList.Empty;

    private string ClosingBracketDebugView => ClosingBracket.Match(none: string.Empty, some: x => x.ToString());

    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();
}

[DebuggerDisplay("\"{DebuggerDisplay}\"")]
public sealed record TriviaList(IImmutableList<Token> Tokens)
{
    public static TriviaList Empty { get; } = new(ImmutableArray<Token>.Empty);

    public bool Equals(TriviaList? other)
        => other is not null && Tokens.SequenceEqual(other.Tokens);

    public override int GetHashCode() => Tokens.Count.GetHashCode();

    private string DebuggerDisplay => Tokens.ConcatToString();
}
