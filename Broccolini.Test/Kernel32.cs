using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Broccolini.Test;

[SupportedOSPlatform("windows")]
internal static class Kernel32
{
    public static string[] GetSectionNames(string filePath)
    {
        var result = GetPrivateProfileStringInternal(appName: null, keyName: null, @default: null, fileName: filePath);
        return result.Length > 0
            ? result[..^1].Split('\0')
            : [];
    }

    public static IEnumerable<string> GetKeysInSection(string filePath, string section)
    {
        var result = GetPrivateProfileStringInternal(appName: section, keyName: null, @default: null, fileName: filePath);
        return result.Length > 0
            ? result[..^1].Split('\0')
            : [];
    }

    public static string GetPrivateProfileString(string filePath, string section, string key, string defaultValue)
        => GetPrivateProfileStringInternal(section, key, defaultValue, filePath);

    private static string GetPrivateProfileStringInternal(string? appName, string? keyName, string? @default, string fileName)
    {
        const int returnBufferSize = 1024;
        var returnBuffer = Marshal.AllocHGlobal(returnBufferSize);

        try
        {
            var returnedBytes = GetPrivateProfileString(appName, keyName, @default, returnBuffer, returnBufferSize, fileName);

            if (Marshal.GetLastPInvokeError() != 0) throw new Win32Exception();

            return Marshal.PtrToStringUni(returnBuffer, (int)returnedBytes);
        }
        finally
        {
            Marshal.FreeHGlobal(returnBuffer);
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetPrivateProfileString(
        string? lpAppName,
        string? lpKeyName,
        string? lpDefault,
        IntPtr lpReturnedString,
        uint nSize,
        string lpFileName);
}
