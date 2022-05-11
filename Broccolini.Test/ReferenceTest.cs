using Broccolini.SemanticModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Versioning;
using System.Text;
using Xunit;
using static Broccolini.Test.Kernel32;
using static Broccolini.Test.TestData;

namespace Broccolini.Test;

public sealed class ReferenceTest
{
    [SkippableTheory]
    [MemberData(nameof(GetSectionNameData))]
    [SupportedOSPlatform("windows")]
    public void ParsesSectionNames(string sectionName, string input)
    {
        Skip.IfNot(OperatingSystem.IsWindows());

        const string arbitraryKey = "key";
        const string arbitraryValue = "value";

        using var temporaryFile = new TemporaryFile();
        File.WriteAllText(temporaryFile.Path, $"{input}\r\n{arbitraryKey} = {arbitraryValue}", Encoding.Unicode);

        Assert.Equal(arbitraryValue, GetPrivateProfileString(temporaryFile.Path, sectionName, arbitraryKey, "DEFAULT VALUE"));
    }

    public static TheoryData<string, string> GetSectionNameData()
        => SectionsWithNames.Select(s => (s.Name, s.Input)).ToTheoryData();

    [SkippableTheory]
    [MemberData(nameof(GetKeyValuePairData))]
    [SupportedOSPlatform("windows")]
    public void ParsesKeyValuePair(string key, string value, string input)
    {
        Skip.IfNot(OperatingSystem.IsWindows());

        const string arbitrarySection = "section";

        using var temporaryFile = new TemporaryFile();
        File.WriteAllText(temporaryFile.Path, $"[{arbitrarySection}]\r\n{input}", Encoding.Unicode);

        Assert.Equal(value, GetPrivateProfileString(temporaryFile.Path, arbitrarySection, key, "DEFAULT VALUE"));
    }

    [Theory]
    [MemberData(nameof(KeysData))]
    [SupportedOSPlatform("windows")]
    public void IgnoresCaseForKey(string keyInFile, string keyUsedForLookup, bool shouldBeEqual)
    {
        Skip.IfNot(OperatingSystem.IsWindows());

        const string arbitrarySection = "section";
        const string arbitraryValue = "value";

        using var temporaryFile = new TemporaryFile();
        File.WriteAllText(temporaryFile.Path, $"[{arbitrarySection}]\r\n{keyInFile} = {arbitraryValue}", Encoding.Unicode);

        if (shouldBeEqual)
        {
            Assert.Equal(arbitraryValue, GetPrivateProfileString(temporaryFile.Path, arbitrarySection, keyUsedForLookup, "DEFAULT VALUE"));
        }
        else
        {
            Assert.Throws<Win32Exception>(() => GetPrivateProfileString(temporaryFile.Path, arbitrarySection, keyUsedForLookup, "DEFAULT VALUE"));
        }
    }

    [Theory]
    [MemberData(nameof(KeysData))]
    [SupportedOSPlatform("windows")]
    public void IgnoresCaseForSectionName(string sectionInFile, string sectionUsedForLookup, bool shouldBeEqual)
    {
        Skip.IfNot(OperatingSystem.IsWindows());

        const string arbitraryKey = "key";
        const string arbitraryValue = "value";

        using var temporaryFile = new TemporaryFile();
        File.WriteAllText(temporaryFile.Path, $"[{sectionInFile}]\r\n{arbitraryKey} = {arbitraryValue}", Encoding.Unicode);

        if (shouldBeEqual)
        {
            Assert.Equal(arbitraryValue, GetPrivateProfileString(temporaryFile.Path, sectionUsedForLookup, arbitraryKey, "DEFAULT VALUE"));
        }
        else
        {
            Assert.Throws<Win32Exception>(() => GetPrivateProfileString(temporaryFile.Path, sectionUsedForLookup, arbitraryKey, "DEFAULT VALUE"));
        }
    }

    [Theory]
    [MemberData(nameof(KeysData))]
    public void SemanticModelIgnoresCaseOfKey(string lhs, string rhs, bool shouldBeEqual)
    {
        Assert.Equal(shouldBeEqual, KeyComparision.KeyEquals(lhs, rhs));
    }

    public static TheoryData<string, string, bool> KeysData()
        => CaseSensitivityInputs.Select(input => (input.Variant1, input.Variant2, input.ShouldBeEqual)).ToTheoryData();

    public static TheoryData<string, string, string> GetKeyValuePairData()
        => KeyValuePairsWithKeyAndValue.Select(s => (s.Key, s.Value, s.Input)).ToTheoryData();

    private sealed class TemporaryFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.GetTempFileName();

        public void Dispose() => File.Delete(Path);
    }
}
