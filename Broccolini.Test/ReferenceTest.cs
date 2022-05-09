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

    public static TheoryData<string, string, string> GetKeyValuePairData()
        => KeyValuePairsWithKeyAndValue.Select(s => (s.Key, s.Value, s.Input)).ToTheoryData();

    private sealed class TemporaryFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.GetTempFileName();

        public void Dispose() => File.Delete(Path);
    }
}
