using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;

namespace Broccolini.Syntax;

public sealed record IniDocument
{
    internal IniDocument(ImmutableArray<SectionChildNode> nodesOutsideSection, ImmutableArray<SectionNode> sections)
    {
        NodesOutsideSection = nodesOutsideSection;
        Sections = sections;
    }

    public static IniDocument Empty { get; } = new(ImmutableArray<SectionChildNode>.Empty, ImmutableArray<SectionNode>.Empty);

    public IImmutableList<SectionChildNode> NodesOutsideSection { get; init; }

    public IImmutableList<SectionNode> Sections { get; init; }

    public bool Equals(IniDocument? other)
        => other is not null
            && NodesOutsideSection.SequenceEqual(other.NodesOutsideSection)
            && Sections.SequenceEqual(other.Sections);

    public override int GetHashCode() => HashCode.Combine(NodesOutsideSection.Count, Sections.Count);

    public override string ToString()
    {
        var visitor = new ToStringVisitor();
        visitor.Visit(NodesOutsideSection);
        visitor.Visit(Sections);
        return visitor.ToString();
    }
}

public abstract record IniNode
{
    private protected IniNode() { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected IniNode(IniNode original)
    {
        NewLine = original.NewLine;
    }

    public Token.NewLine? NewLine { get; init; }

    public abstract void Accept(IIniNodeVisitor visitor);

    public override string ToString()
    {
        var visitor = new ToStringVisitor();
        Accept(visitor);
        return visitor.ToString();
    }

    private protected abstract void InternalImplementorsOnly();
}

public abstract record SectionChildNode : IniNode
{
    private protected SectionChildNode() { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected SectionChildNode(SectionChildNode original) : base(original) { }

    public override string ToString() => base.ToString();
}

/// <summary>A key-value pair: <c>key = value</c>. Use <see cref="SyntaxFactory.KeyValue"/> to create this node.</summary>
[DebuggerDisplay("{Key,nq}{EqualsSign,nq}{Value,nq}")]
public sealed record KeyValueNode : SectionChildNode
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public KeyValueNode(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; init; }

    public string Value { get; init; }

    /// <summary>Leading whitespace.</summary>
    public Token.WhiteSpace? LeadingTrivia { get; init; }

    /// <summary>Whitespace between key and equals sign.</summary>
    public Token.WhiteSpace? TriviaBeforeEqualsSign { get; init; }

    public Token.EqualsSign EqualsSign { get; init; } = new();

    /// <summary>Whitespace between equals sign and value.</summary>
    public Token.WhiteSpace? TriviaAfterEqualsSign { get; init; }

    /// <summary>Opening and closing quote when value is quoted.</summary>
    public Token.Quote? Quote { get; init; }

    /// <summary>Whitespace after value.</summary>
    public Token.WhiteSpace? TrailingTrivia { get; init; }

    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();

    private protected override void InternalImplementorsOnly() { }
}

/// <summary>A line that can't be recognized as one of the other node types.</summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed record UnrecognizedNode : SectionChildNode
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public UnrecognizedNode(IImmutableList<Token> tokens)
    {
        Tokens = tokens;
    }

    public IImmutableList<Token> Tokens { get; init; }

    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => string.Concat(Tokens);

    public bool Equals(UnrecognizedNode? other) => other is not null && Tokens.SequenceEqual(other.Tokens);

    public override int GetHashCode() => Tokens.Count.GetHashCode();

    internal bool IsBlank() => Tokens.All(static token => token is Token.WhiteSpace or Token.NewLine);

    private protected override void InternalImplementorsOnly() { }
}

/// <summary>A comment: <c>; comment</c>.</summary>
[DebuggerDisplay("{Semicolon,nq}{Text,nq}")]
public sealed record CommentNode : SectionChildNode
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public CommentNode(string text)
    {
        Text = text;
    }

    public string Text { get; init; }

    /// <summary>Leading whitespace.</summary>
    public Token.WhiteSpace? LeadingTrivia { get; init; }

    public Token.Semicolon Semicolon { get; init; } = new();

    /// <summary>Whitespace between semicolon and text.</summary>
    public Token.WhiteSpace? TriviaAfterSemicolon { get; init; }

    /// <summary>Trailing whitespace.</summary>
    public Token.WhiteSpace? TrailingTrivia { get; init; }

    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();

    private protected override void InternalImplementorsOnly() { }
}

/// <summary>A section:
/// <code>[section]
/// key = value</code>
/// Use <see cref="SyntaxFactory.Section"/> to create this node.</summary>
[DebuggerDisplay("{OpeningBracket,nq}{Name,nq}{ClosingBracketDebugView,nq}")]
public sealed record SectionNode : IniNode
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public SectionNode(string name, IImmutableList<SectionChildNode> children)
    {
        Name = name;
        Children = children;
    }

    public string Name { get; init; }

    public IImmutableList<SectionChildNode> Children { get; init; }

    /// <summary>Leading whitespace.</summary>
    public Token.WhiteSpace? LeadingTrivia { get; init; }

    public Token.OpeningBracket OpeningBracket { get; init; } = new();

    /// <summary>Whitespace between opening bracket and section name.</summary>
    public Token.WhiteSpace? TriviaAfterOpeningBracket { get; init; }

    /// <summary>Whitespace between section name and closing bracket.</summary>
    public Token.WhiteSpace? TriviaBeforeClosingBracket { get; init; }

    public Token.ClosingBracket? ClosingBracket { get; init; } = new();

    /// <summary>Trailing whitespace and garbage after the closing bracket.</summary>
    public IImmutableList<Token> TrailingTrivia { get; init; } = ImmutableArray<Token>.Empty;

    internal Token.NewLine? NewLineHint { get; init; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string ClosingBracketDebugView => ClosingBracket?.ToString() ?? string.Empty;

    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public bool Equals(SectionNode? other)
        => other is not null
           && Name == other.Name
           && Children.SequenceEqual(other.Children)
           && LeadingTrivia == other.LeadingTrivia
           && OpeningBracket == other.OpeningBracket
           && TriviaAfterOpeningBracket == other.TriviaAfterOpeningBracket
           && TriviaBeforeClosingBracket == other.TriviaBeforeClosingBracket
           && ClosingBracket == other.ClosingBracket
           && TrailingTrivia.SequenceEqual(other.TrailingTrivia)
           && NewLine == other.NewLine;

    public override int GetHashCode()
        => HashCode.Combine(
            Name,
            Children.Count,
            LeadingTrivia,
            OpeningBracket,
            TriviaAfterOpeningBracket,
            TriviaBeforeClosingBracket,
            ClosingBracket,
            HashCode.Combine(
                TrailingTrivia,
                NewLine));

    public override string ToString() => base.ToString();

    private protected override void InternalImplementorsOnly() { }
}
