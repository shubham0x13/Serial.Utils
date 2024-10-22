using System.Management;
using System.Collections.Generic;

namespace Serial.Utils;

public class SerialPortFinder
{
    public static string[] GetPortsByVid(string vid)
    {
        return QuerySerialPorts($"DeviceID LIKE '%VID_{vid}%'");
    }

    public static string[] GetPortsByPid(string pid)
    {
        return QuerySerialPorts($"DeviceID LIKE '%PID_{pid}%'");
    }

    public static string[] GetPortsByVidAndPid(string vid, string pid)
    {
        return QuerySerialPorts($"DeviceID LIKE '%VID_{vid}%' AND DeviceID LIKE '%PID_{pid}%'");
    }

    private static string[] QuerySerialPorts(string condition)
    {
        string query = $"SELECT * FROM Win32_PnPEntity WHERE {condition}";

        using ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
        using ManagementObjectCollection collection = searcher.Get();

        List<string> ports = new List<string>();

        foreach (ManagementBaseObject obj in collection)
        {
            if (obj["Name"] is string name)
            {
                string? port = SerialPortUtils.ExtractPortNameFromDeviceName(name);
                if (port != null)
                    ports.Add(port);
            }
        }

        return ports.ToArray();
    }
}
