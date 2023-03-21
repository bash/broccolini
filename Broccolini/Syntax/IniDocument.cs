using System.Collections.Immutable;
using System.Diagnostics;

namespace Broccolini.Syntax;

public sealed record IniDocument(IImmutableList<SectionChildNode> NodesOutsideSection, IImmutableList<SectionNode> Sections)
{
    public static IniDocument Empty { get; } = new(ImmutableArray<SectionChildNode>.Empty, ImmutableArray<SectionNode>.Empty);

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

internal static class IniDocumentExtensions
{
    public static IEnumerable<IniNode> GetNodes(this IniDocument document)
        => ((IEnumerable<IniNode>)document.NodesOutsideSection).Concat(document.Sections);
}

public abstract record IniNode
{
    internal IniNode() { }

    public Token.NewLine? NewLine { get; init; }

    public abstract void Accept(IIniNodeVisitor visitor);

    public override string ToString()
    {
        var visitor = new ToStringVisitor();
        Accept(visitor);
        return visitor.ToString();
    }
}

public abstract record SectionChildNode : IniNode
{
    public override string ToString() => base.ToString();
}

/// <summary>A key-value pair: <c>key = value</c>. Use <see cref="SyntaxFactory.KeyValue"/> to create this node.</summary>
[DebuggerDisplay("{Key}{EqualsSign}{Value}")]
public sealed record KeyValueNode(string Key, string Value) : SectionChildNode
{
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
}

/// <summary>A line that can't be recognized as one of the other node types.</summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed record UnrecognizedNode(IImmutableList<Token> Tokens) : SectionChildNode
{
    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => string.Concat(Tokens);

    public bool Equals(UnrecognizedNode? other) => other is not null && Tokens.SequenceEqual(other.Tokens);

    public override int GetHashCode() => Tokens.Count.GetHashCode();

    internal bool IsBlank() => Tokens.All(static token => token is Token.WhiteSpace or Token.NewLine);
}

/// <summary>A comment: <c>; comment</c>.</summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed record CommentNode(string Text) : SectionChildNode
{
    /// <summary>Leading whitespace.</summary>
    public Token.WhiteSpace? LeadingTrivia { get; init; }

    public Token.Semicolon Semicolon { get; init; } = new();

    /// <summary>Whitespace between semicolon and text.</summary>
    public Token.WhiteSpace? TriviaAfterSemicolon { get; init; }

    /// <summary>Trailing whitespace.</summary>
    public Token.WhiteSpace? TrailingTrivia { get; init; }

    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();
}

/// <summary>A section:
/// <code>[section]
/// key = value</code>
/// Use <see cref="SyntaxFactory.Section"/> to create this node.</summary>
[DebuggerDisplay("{OpeningBracket}{Name}{ClosingBracketDebugView}")]
public sealed record SectionNode(string Name, IImmutableList<SectionChildNode> Children) : IniNode
{
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
}
