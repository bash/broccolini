using System.Collections;

namespace Broccolini.SemanticModel;

internal sealed record Document(IReadOnlyDictionary<string, ISection> Sections) : IDocument
{
    public IEnumerator<KeyValuePair<string, ISection>> GetEnumerator() => Sections.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => Sections.Count;

    public bool ContainsKey(string key) => Sections.ContainsKey(key);

#if NET6_0_OR_GREATER
    public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out ISection value) => Sections.TryGetValue(key, out value);
#else
    public bool TryGetValue(string key, out ISection value) => Sections.TryGetValue(key, out value);
#endif

    public ISection this[string key] => Sections[key];

    public IEnumerable<string> Keys => Sections.Keys;

    public IEnumerable<ISection> Values => Sections.Values;
}

internal sealed record Section(string Name, IReadOnlyDictionary<string, string> Entries) : ISection
{
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => Entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => Entries.Count;

    public bool ContainsKey(string key) => Entries.ContainsKey(key);

#if NET6_0_OR_GREATER
    public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out string value) => Entries.TryGetValue(key, out value);
#else
    public bool TryGetValue(string key, out string value) => Entries.TryGetValue(key, out value);
#endif

    public string this[string key] => Entries[key];

    public IEnumerable<string> Keys => Entries.Keys;

    public IEnumerable<string> Values => Entries.Values;
}
