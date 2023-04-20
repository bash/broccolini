using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;

namespace Broccolini.Syntax;

public sealed record IniDocument
{
    internal IniDocument(ImmutableArray<SectionChildIniNode> nodesOutsideSection, ImmutableArray<SectionIniNode> sections)
    {
        NodesOutsideSection = nodesOutsideSection;
        Sections = sections;
    }

    public static IniDocument Empty { get; } = new(ImmutableArray<SectionChildIniNode>.Empty, ImmutableArray<SectionIniNode>.Empty);

    public IImmutableList<SectionChildIniNode> NodesOutsideSection { get; init; }

    public IImmutableList<SectionIniNode> Sections { get; init; }

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

    public IniToken.NewLine? NewLine { get; init; }

    public abstract void Accept(IIniNodeVisitor visitor);

    public override string ToString()
    {
        var visitor = new ToStringVisitor();
        Accept(visitor);
        return visitor.ToString();
    }

    private protected abstract void InternalImplementorsOnly();
}

public abstract record SectionChildIniNode : IniNode
{
    private protected SectionChildIniNode() { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected SectionChildIniNode(SectionChildIniNode original) : base(original) { }

    public override string ToString() => base.ToString();
}

/// <summary>A key-value pair: <c>key = value</c>. Use <see cref="IniSyntaxFactory.KeyValue"/> to create this node.</summary>
[DebuggerDisplay("{Key,nq}{EqualsSign,nq}{Value,nq}")]
public sealed record KeyValueIniNode : SectionChildIniNode
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public KeyValueIniNode(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; init; }

    public string Value { get; init; }

    /// <summary>Leading whitespace.</summary>
    public IniToken.WhiteSpace? LeadingTrivia { get; init; }

    /// <summary>Whitespace between key and equals sign.</summary>
    public IniToken.WhiteSpace? TriviaBeforeEqualsSign { get; init; }

    public IniToken.EqualsSign EqualsSign { get; init; } = new();

    /// <summary>Whitespace between equals sign and value.</summary>
    public IniToken.WhiteSpace? TriviaAfterEqualsSign { get; init; }

    /// <summary>Opening and closing quote when value is quoted.</summary>
    public IniToken.Quote? Quote { get; init; }

    /// <summary>Whitespace after value.</summary>
    public IniToken.WhiteSpace? TrailingTrivia { get; init; }

    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();

    private protected override void InternalImplementorsOnly() { }
}

/// <summary>A line that can't be recognized as one of the other node types.</summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed record UnrecognizedIniNode : SectionChildIniNode
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public UnrecognizedIniNode(IImmutableList<IniToken> tokens)
    {
        Tokens = tokens;
    }

    public IImmutableList<IniToken> Tokens { get; init; }

    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => string.Concat(Tokens);

    public bool Equals(UnrecognizedIniNode? other) => other is not null && Tokens.SequenceEqual(other.Tokens);

    public override int GetHashCode() => Tokens.Count.GetHashCode();

    internal bool IsBlank() => Tokens.All(static token => token is IniToken.WhiteSpace or IniToken.NewLine);

    private protected override void InternalImplementorsOnly() { }
}

/// <summary>A comment: <c>; comment</c>.</summary>
[DebuggerDisplay("{Semicolon,nq}{Text,nq}")]
public sealed record CommentIniNode : SectionChildIniNode
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public CommentIniNode(string text)
    {
        Text = text;
    }

    public string Text { get; init; }

    /// <summary>Leading whitespace.</summary>
    public IniToken.WhiteSpace? LeadingTrivia { get; init; }

    public IniToken.Semicolon Semicolon { get; init; } = new();

    /// <summary>Whitespace between semicolon and text.</summary>
    public IniToken.WhiteSpace? TriviaAfterSemicolon { get; init; }

    /// <summary>Trailing whitespace.</summary>
    public IniToken.WhiteSpace? TrailingTrivia { get; init; }

    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();

    private protected override void InternalImplementorsOnly() { }
}

/// <summary>A section:
/// <code>[section]
/// key = value</code>
/// Use <see cref="IniSyntaxFactory.Section"/> to create this node.</summary>
[DebuggerDisplay("{OpeningBracket,nq}{Name,nq}{ClosingBracketDebugView,nq}")]
public sealed record SectionIniNode : IniNode
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public SectionIniNode(string name, IImmutableList<SectionChildIniNode> children)
    {
        Name = name;
        Children = children;
    }

    public string Name { get; init; }

    public IImmutableList<SectionChildIniNode> Children { get; init; }

    /// <summary>Leading whitespace.</summary>
    public IniToken.WhiteSpace? LeadingTrivia { get; init; }

    public IniToken.OpeningBracket OpeningBracket { get; init; } = new();

    /// <summary>Whitespace between opening bracket and section name.</summary>
    public IniToken.WhiteSpace? TriviaAfterOpeningBracket { get; init; }

    /// <summary>Whitespace between section name and closing bracket.</summary>
    public IniToken.WhiteSpace? TriviaBeforeClosingBracket { get; init; }

    public IniToken.ClosingBracket? ClosingBracket { get; init; } = new();

    /// <summary>Trailing whitespace and garbage after the closing bracket.</summary>
    public IImmutableList<IniToken> TrailingTrivia { get; init; } = ImmutableArray<IniToken>.Empty;

    internal IniToken.NewLine? NewLineHint { get; init; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string ClosingBracketDebugView => ClosingBracket?.ToString() ?? string.Empty;

    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public bool Equals(SectionIniNode? other)
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
