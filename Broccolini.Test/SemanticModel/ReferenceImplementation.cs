using System.Collections.Immutable;
using System.Runtime.Versioning;
using Broccolini.SemanticModel;
using static Broccolini.SemanticModel.KeyComparision;
using static Broccolini.Test.Kernel32;

namespace Broccolini.Test.SemanticModel;

[SupportedOSPlatform("windows")]
public sealed class ReferenceImplementation
{
    public static IIniDocument CreateReferenceDocument(string input)
    {
        var temporaryFile = Path.GetTempFileName();

        try
        {
            File.WriteAllText(temporaryFile, input);
            return CreateDocumentFromWin32Api(temporaryFile);
        }
        finally
        {
            File.Delete(temporaryFile);
        }
    }

    private static IIniDocument CreateDocumentFromWin32Api(string filePath)
    {
        IIniSection CreateSection(string sectionName)
            => new Section(sectionName, GetKeysInSection(filePath, sectionName)
                .Distinct(KeyComparer)
                .ToImmutableDictionary(Identity, GetValueForKey(sectionName), keyComparer: KeyComparer));

        Func<string, string> GetValueForKey(string sectionName)
            => key
                => GetPrivateProfileString(filePath, sectionName, key, "DEFAULT VALUE");

        return new Document(
            GetSectionNames(filePath)
                .Distinct(KeyComparer)
                .ToImmutableDictionary(Identity, CreateSection, keyComparer: KeyComparer));
    }
}
