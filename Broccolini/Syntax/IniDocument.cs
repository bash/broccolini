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

    public Token? LineBreak { get; init; }

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
    /// <summary>Leading whitespace</summary>
    public TriviaList LeadingTrivia { get; init; } = TriviaList.Empty;

    public TriviaList TriviaBeforeEqualsSign { get; init; } = TriviaList.Empty;

    public Token EqualsSign { get; init; } = new Token.EqualsSign();

    public TriviaList TriviaAfterEqualsSign { get; init; } = TriviaList.Empty;

    public Token? OpeningQuote { get; init; }

    public Token? ClosingQuote { get; init; }

    /// <summary>Trailing whitespace and line breaks.</summary>
    public TriviaList TrailingTrivia { get; init; } = TriviaList.Empty;

    public override void Accept(IIniNodeVisitor visitor) => visitor.Visit(this);

    public override string ToString() => base.ToString();
}

/// <summary>A comment or an unrecognized line.</summary>
[DebuggerDisplay("{Value}")]
public sealed record TriviaNode(TriviaList Value) : SectionChildNode
{
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
    /// <summary>Leading whitespace</summary>
    public TriviaList LeadingTrivia { get; init; } = TriviaList.Empty;

    public Token OpeningBracket { get; init; } = new Token.OpeningBracket();

    public TriviaList TriviaAfterOpeningBracket { get; init; } = TriviaList.Empty;

    public TriviaList TriviaBeforeClosingBracket { get; init; } = TriviaList.Empty;

    public Token? ClosingBracket { get; init; } = new Token.ClosingBracket();

    /// <summary>Trailing whitespace, line breaks and garbage after the closing bracket.</summary>
    public TriviaList TrailingTrivia { get; init; } = TriviaList.Empty;

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
           && TrailingTrivia == other.TrailingTrivia
           && LineBreak == other.LineBreak;

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
                LineBreak));

    public override string ToString() => base.ToString();
}

[DebuggerDisplay("\"{DebuggerDisplay}\"")]
public sealed record TriviaList(IImmutableList<Token> Tokens)
{
    public TriviaList(params Token[] tokens)
        : this(tokens.ToImmutableArray())
    {
    }

    public static TriviaList Empty { get; } = new(ImmutableArray<Token>.Empty);

    public bool Equals(TriviaList? other)
        => other is not null && Tokens.SequenceEqual(other.Tokens);

    public override int GetHashCode() => Tokens.Count.GetHashCode();

    private string DebuggerDisplay => string.Concat(Tokens);
}
