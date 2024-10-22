using System.Text.RegularExpressions;

namespace Serial.Utils;

internal static class SerialPortUtils
{
    private static readonly Regex _comPortRegex = new Regex(@"\((COM\d+)\)", RegexOptions.Compiled);

    public static string? ExtractPortNameFromDeviceName(string deviceName)
    {
        Match match = _comPortRegex.Match(deviceName);
        return match.Success
            ? match.Groups[1].Value
            : null;
    }
}
