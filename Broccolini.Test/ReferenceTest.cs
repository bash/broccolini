using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using static Broccolini.Test.TestData;

namespace Broccolini.Test;

public sealed class ReferenceTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ReferenceTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

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

        Assert.Equal(arbitraryValue, GetPrivateProfileString(temporaryFile.Path, sectionName, arbitraryKey));
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

        _testOutputHelper.WriteLine(GetPrivateProfileString(temporaryFile.Path, arbitrarySection, null));

        Assert.Equal(value, GetPrivateProfileString(temporaryFile.Path, arbitrarySection, key));
    }

    public static TheoryData<string, string, string> GetKeyValuePairData()
        => KeyValuePairsWithKeyAndValue.Select(s => (s.Key, s.Value, s.Input)).ToTheoryData();

    [SupportedOSPlatform("windows")]
    private static string GetPrivateProfileString(string filePath, string? section, string? key)
    {
        var stringBuilder = new StringBuilder(1024);
        _ = GetPrivateProfileString(section, key, "default value", stringBuilder, (uint)stringBuilder.Capacity, filePath);

        if (Marshal.GetLastWin32Error() != 0)
        {
            throw new Win32Exception();
        }

        return stringBuilder.ToString();
    }

    private sealed class TemporaryFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.GetTempFileName();

        public void Dispose() => File.Delete(Path);
    }

    [SupportedOSPlatform("windows")]
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetPrivateProfileString(
        string? lpAppName,
        string? lpKeyName,
        string lpDefault,
        StringBuilder lpReturnedString,
        uint nSize,
        string lpFileName);
}
