using Broccolini.Syntax;
using System.Diagnostics.Contracts;
using static Broccolini.Syntax.IniSyntaxFactory;

// ReSharper disable once CheckNamespace
namespace Broccolini;

public static partial class EditingExtensions
{
    /// <summary>Updates the value of a key-value node.</summary>
    /// <param name="keyValueNode" />
    /// <param name="value">The value may contain anything except newlines. Quotes are automatically added as needed to preserve whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when the updated node would result in something different when parsed back.</exception>
    [Pure]
    public static KeyValueIniNode WithValue(this KeyValueIniNode keyValueNode, string value)
    {
        var nodeWithNewValue = KeyValue(keyValueNode.Key, value);
        return keyValueNode with
        {
            Value = nodeWithNewValue.Value,
            Quote = keyValueNode.Quote ?? nodeWithNewValue.Quote,
        };
    }
}
