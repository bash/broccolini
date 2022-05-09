using System.Collections.Immutable;
using System.Runtime.Versioning;
using Broccolini.SemanticModel;
using static Broccolini.Test.Kernel32;

namespace Broccolini.Test.SemanticModel;

[SupportedOSPlatform("windows")]
public sealed class ReferenceImplementation
{
    public static IDocument CreateReferenceDocument(string input)
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

    private static IDocument CreateDocumentFromWin32Api(string filePath)
    {
        ISection CreateSection(string sectionName)
            => new Section(sectionName, GetKeysInSection(filePath, sectionName).ToImmutableDictionary(Identity, GetValueForKey(sectionName)));

        Func<string, string> GetValueForKey(string sectionName)
            => key
                => GetPrivateProfileString(filePath, sectionName, key, "DEFAULT VALUE");

        return new Document(
            GetSectionNames(filePath)
                .Distinct()
                .ToImmutableDictionary(Identity, CreateSection));
    }
}
