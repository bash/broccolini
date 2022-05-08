using System.Diagnostics.Contracts;
using Broccolini.Syntax;

namespace Broccolini.Editing;

public static class EditingExtensions
{
    /// <summary>Appends or updates a section with the given name.</summary>
    [Pure]
    public static IniDocument WithAppendedOrUpdatedSection(this IniDocument document, string sectionName, Func<SectionNode, SectionNode> updateSection) => throw new NotImplementedException();

    /// <summary>Updates a section with the given name. Does nothing when the section does not exist.</summary>
    [Pure]
    public static IniDocument WithUpdatedSection(this IniDocument document, string sectionName, Func<SectionNode, SectionNode> updateSection) => throw new NotImplementedException();

    /// <summary>Removes all sections with the given name. Preserves leading and trailing trivia.</summary>
    [Pure]
    public static IniDocument WithRemovedSection(this IniDocument document, string sectionName) => throw new NotImplementedException();

    /// <summary>Appends or updates a key-value entry.</summary>
    [Pure]
    public static SectionNode WithAppendedOrUpdatedEntry(this SectionNode sectionNode, string key, string value) => throw new NotImplementedException();

    /// <summary>Removes an entry from the section.</summary>
    [Pure]
    public static SectionNode WithRemovedEntry(this SectionNode sectionNode, string key) => throw new NotImplementedException();
}
