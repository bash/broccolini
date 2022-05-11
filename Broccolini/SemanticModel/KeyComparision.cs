namespace Broccolini.SemanticModel;

internal static class KeyComparision
{
    public static StringComparer KeyComparer { get; } = StringComparer.OrdinalIgnoreCase;

    public static bool KeyEquals(string lhs, string rhs) => KeyComparer.Equals(lhs, rhs);
}
