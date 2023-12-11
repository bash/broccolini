using System.Collections;

namespace Broccolini.SemanticModel;

internal sealed class Document(IReadOnlyDictionary<string, IIniSection> sections) : IIniDocument
{
    public IEnumerator<KeyValuePair<string, IIniSection>> GetEnumerator() => sections.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => sections.Count;

    public bool ContainsKey(string key) => sections.ContainsKey(key);

#if NET6_0_OR_GREATER
    public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out IIniSection value) => sections.TryGetValue(key, out value);
#else
    public bool TryGetValue(string key, out IIniSection value) => sections.TryGetValue(key, out value);
#endif

    public IIniSection this[string key] => sections[key];

    public IEnumerable<string> Keys => sections.Keys;

    public IEnumerable<IIniSection> Values => sections.Values;
}

internal sealed class Section(string name, IReadOnlyDictionary<string, string> entries) : IIniSection
{
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public string Name { get; } = name;

    public int Count => entries.Count;

    public bool ContainsKey(string key) => entries.ContainsKey(key);

#if NET6_0_OR_GREATER
    public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out string value) => entries.TryGetValue(key, out value);
#else
    public bool TryGetValue(string key, out string value) => entries.TryGetValue(key, out value);
#endif

    public string this[string key] => entries[key];

    public IEnumerable<string> Keys => entries.Keys;

    public IEnumerable<string> Values => entries.Values;
}
