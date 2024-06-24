using System.ComponentModel;
using System.Diagnostics;

namespace Broccolini.Syntax;

/// <summary><para>An immutable AST representation of an INI document.</para>
/// <para>Use <see cref="IniParser.Parse"/> to parse an INI document from a string.</para>
/// <para>The document can be converted back its string representation using <see cref="ToString"/> including all trivia (comments, whitespace and unrecognized lines).</para></summary>
public sealed record IniDocument
{
    internal IniDocument(ImmutableArray<SectionChildIniNode> nodesOutsideSection, ImmutableArray<SectionIniNode> sections)
    {
        NodesOutsideSection = nodesOutsideSection;
        Sections = sections;
    }

    /// <summary>An empty document.</summary>
    public static IniDocument Empty { get; } = new(ImmutableArray<SectionChildIniNode>.Empty, ImmutableArray<SectionIniNode>.Empty);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IImmutableList<SectionChildIniNode> NodesOutsideSection { get; init; }

    /// <remarks>Prefer
    /// <see cref="EditingExtensions.WithSection"/>,
    /// <see cref="EditingExtensions.UpdateSection"/>, or
    /// <see cref="EditingExtensions.RemoveSection"/>
    /// to edit a document's sections over manually editing the sections.</remarks>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IImmutableList<SectionIniNode> Sections { get; init; }

    public bool Equals(IniDocument? other)
        => other is not null
            && NodesOutsideSection.SequenceEqual(other.NodesOutsideSection)
            && Sections.SequenceEqual(other.Sections);

    public override int GetHashCode() => HashCode.Combine(NodesOutsideSection.Count, Sections.Count);

    /// <summary>Converts this INI document back to its string representation.</summary>
    public override string ToString()
    {
        var visitor = new ToStringVisitor();
        visitor.Visit(NodesOutsideSection);
        visitor.Visit(Sections);
        return visitor.ToString();
    }
}

[EditorBrowsable(EditorBrowsableState.Advanced)]
public abstract record IniNode
{
    private protected IniNode() { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected IniNode(IniNode original)
    {
        NewLine = original.NewLine;
        LeadingTrivia = original.LeadingTrivia;
        TrailingTrivia = original.TrailingTrivia;
    }

    /// <summary>Leading whitespace and empty lines.</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IImmutableList<IniToken> LeadingTrivia { get; init; } = ImmutableArray<IniToken>.Empty;

    /// <summary>Trailing whitespace and empty lines.</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IImmutableList<IniToken> TrailingTrivia { get; init; } = ImmutableArray<IniToken>.Empty;

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IniToken.NewLine? NewLine { get; init; }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public abstract void Accept(IIniNodeVisitor visitor);

    /// <summary>Converts this INI node back to its string representation.</summary>
    public override string ToString()
    {
        var visitor = new ToStringVisitor();
        Accept(visitor);
        return visitor.ToString();
    }

    public virtual bool Equals(IniNode? other)
        => other is not null
            && EqualityContract == other.EqualityContract
            && LeadingTrivia.SequenceEqual(other.LeadingTrivia)
            && TrailingTrivia.SequenceEqual(other.TrailingTrivia)
            && NewLine == other.NewLine;

    public override int GetHashCode()
        => HashCode.Combine(
            EqualityContract,
            LeadingTrivia.Count,
            TrailingTrivia.Count,
            NewLine);

    private protected abstract void InternalImplementorsOnly();
}

[EditorBrowsable(EditorBrowsableState.Advanced)]
public abstract record SectionChildIniNode : IniNode
{
    private protected SectionChildIniNode() { }

    [EditorBrowsable(EditorBrowsableState.Never)]
    protected SectionChildIniNode(SectionChildIniNode original) : base(original) { }

    public override string ToString() => base.ToString();
}

/// <summary>A key-value pair: <c>key = value</c>. Use <see cref="IniSyntaxFactory.KeyValue"/> to create this node.</summary>
[DebuggerDisplay("{Key,nq}{EqualsSign,nq}{Value,nq}")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
public sealed record KeyValueIniNode : SectionChildIniNode
{
    /// <summary>Creates a key-value node without validating the parts.
    /// Prefer <see cref="IniSyntaxFactory.KeyValue"/> over this constructor.</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public KeyValueIniNode(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; init; }

    public string Value { get; init; }

    /// <summary>Whitespace between key and equals sign.</summary>
    public IniToken.WhiteSpace? TriviaBeforeEqualsSign { get; init; }

    public IniToken.EqualsSign EqualsSign { get; init; } = new();

    /// <summary>Whitespace between equals sign and value.</summary>
    public IniToken.WhiteSpace? TriviaAfterEqualsSign { get; init; }

    /// <summary>Opening and closing quote when value is quoted.</summary>
    public IniToken.Quote? Quote { get; init; }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();

    private protected override void InternalImplementorsOnly() { }
}

/// <summary>A line that can't be recognized as one of the other node types.</summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
public sealed record UnrecognizedIniNode : SectionChildIniNode
{
    /// <summary>Creates an unrecognized node without validating the parts.</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public UnrecognizedIniNode(IImmutableList<IniToken> tokens)
    {
        Tokens = tokens;
    }

    public IImmutableList<IniToken> Tokens { get; init; }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
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
[EditorBrowsable(EditorBrowsableState.Advanced)]
public sealed record CommentIniNode : SectionChildIniNode
{
    /// <summary>Creates a comment node without validating the parts.</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public CommentIniNode(string text)
    {
        Text = text;
    }

    public string Text { get; init; }

    public IniToken.Semicolon Semicolon { get; init; } = new();

    /// <summary>Whitespace between semicolon and text.</summary>
    public IniToken.WhiteSpace? TriviaAfterSemicolon { get; init; }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();

    private protected override void InternalImplementorsOnly() { }
}

/// <summary>A section:
/// <code>[section]
/// key = value</code>
/// Use <see cref="IniSyntaxFactory.Section"/> to create this node.</summary>
[DebuggerDisplay("{Header,nq}")]
public sealed record SectionIniNode : IniNode
{
    /// <summary>Creates a section node without validating the parts.
    /// Prefer <see cref="IniSyntaxFactory.Section"/> over this constructor.</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public SectionIniNode(IniSectionHeader header, IImmutableList<SectionChildIniNode> children)
    {
        Header = header;
        Children = children;
    }

    public string Name => Header.Name;

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IniSectionHeader Header { get; init; }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IImmutableList<SectionChildIniNode> Children { get; init; }

    // intentionally omitted from equality as it is only set for a short time during editing.
    internal IniToken.NewLine? NewLineHint { get; init; }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public bool Equals(SectionIniNode? other)
        => other is not null
            && base.Equals(other)
            && Header == other.Header
            && Children.SequenceEqual(other.Children);

    public override int GetHashCode()
        => HashCode.Combine(
            base.GetHashCode(),
            Header,
            Children.Count);

    public override string ToString() => base.ToString();

    private protected override void InternalImplementorsOnly() { }
}

/// <summary>A section header:
/// <code>[section]</code></summary>
[DebuggerDisplay("{OpeningBracket,nq}{Name,nq}{ClosingBracketDebugView,nq}")]
public sealed record IniSectionHeader
{
    /// <summary>Creates a section node without validating the parts.
    /// Prefer <see cref="IniSyntaxFactory.Section"/> over this constructor.</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IniSectionHeader(string name)
    {
        Name = name;
    }

    public string Name { get; init; }


    /// <summary>Leading whitespace.</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IniToken.WhiteSpace? LeadingTrivia { get; init; }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IniToken.OpeningBracket OpeningBracket { get; init; } = new();

    /// <summary>Whitespace between opening bracket and section name.</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IniToken.WhiteSpace? TriviaAfterOpeningBracket { get; init; }

    /// <summary>Whitespace between section name and closing bracket.</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IniToken.WhiteSpace? TriviaBeforeClosingBracket { get; init; }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IniToken.ClosingBracket? ClosingBracket { get; init; } = new();

    /// <summary>Trailing whitespace and garbage after the closing bracket.</summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public IImmutableList<IniToken> TrailingTrivia { get; init; } = ImmutableArray<IniToken>.Empty;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string ClosingBracketDebugView => ClosingBracket?.ToString() ?? string.Empty;

    public override string ToString()
    {
        var visitor = new ToStringVisitor();
        visitor.Visit(this);
        return visitor.ToString();
    }

    public bool Equals(IniSectionHeader? other)
        => other is not null
           && Name == other.Name
           && LeadingTrivia == other.LeadingTrivia
           && OpeningBracket == other.OpeningBracket
           && TriviaAfterOpeningBracket == other.TriviaAfterOpeningBracket
           && TriviaBeforeClosingBracket == other.TriviaBeforeClosingBracket
           && ClosingBracket == other.ClosingBracket
           && TrailingTrivia.SequenceEqual(other.TrailingTrivia);

    public override int GetHashCode()
        => HashCode.Combine(
            Name,
            LeadingTrivia,
            OpeningBracket,
            TriviaAfterOpeningBracket,
            TriviaBeforeClosingBracket,
            ClosingBracket,
            TrailingTrivia);
}
