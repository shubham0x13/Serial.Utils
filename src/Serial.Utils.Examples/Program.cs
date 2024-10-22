namespace Serial.Utils.Examples;

internal class Program
{
    static void Main(string[] args)
    {
        port.PortConnected += Port_PortConnected;
        port.PortDisconnected += Port_PortDisconnected;
        port.PortOpened += Port_PortOpened;
        port.PortClosed += Port_PortClosed;

        port.AutoConnect = true;
        port.TryConnect();

        Console.ReadKey(true);

        port.Disconnect();

        port.PortConnected -= Port_PortConnected;
        port.PortDisconnected -= Port_PortDisconnected;
        port.PortOpened -= Port_PortOpened;
        port.PortClosed -= Port_PortClosed;

        // Find/Filter Ports By VID and PID
        Console.WriteLine("Finding ports by VID and PID...");

        string vid = "your_vid_here"; // Replace with actual VID
        string pid = "your_pid_here"; // Replace with actual PID

        string[] portsByVID = SerialPortFinder.GetPortsByVID(vid);
        Console.WriteLine($"Ports with VID '{vid}': {string.Join(", ", portsByVID)}");

        string[] portsByPID = SerialPortFinder.GetPortsByPID(pid);
        Console.WriteLine($"Ports with PID '{pid}': {string.Join(", ", portsByPID)}");

        string[] portsByVIDAndPID = SerialPortFinder.GetPortsByVIDAndPID(vid, pid);
        Console.WriteLine($"Ports with VID '{vid}' and PID '{pid}': {string.Join(", ", portsByVIDAndPID)}");

        // Watch Port Connect/Disconnect Events
        Console.WriteLine("\nStarting to watch for port connect/disconnect events...");

        SerialPortWatcher watcher = new SerialPortWatcher();
        watcher.PortConnected += (s, e) => Console.WriteLine($"Port connected: {e.PortName}");
        watcher.PortDisconnected += (s, e) => Console.WriteLine($"Port disconnected: {e.PortName}");
        watcher.Start();

        Console.WriteLine("Press any key to stop watching...");
        Console.ReadKey(true);

        watcher.Stop();
        watcher.Dispose();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(true);
    }

    private static void Port_PortConnected(object? sender, SerialPortEventArgs e)
    {
        Console.WriteLine($"Port connected: {e.PortName}");
    }

    private static void Port_PortDisconnected(object? sender, SerialPortEventArgs e)
    {
        Console.WriteLine($"Port disconnected: {e.PortName}");
    }

    private static void Port_PortOpened(object? sender, SerialPortEventArgs e)
    {
        Console.WriteLine($"Port opened: {e.PortName}");
    }

    private static void Port_PortClosed(object? sender, SerialPortEventArgs e)
    {
        Console.WriteLine($"Port closed: {e.PortName}");
    }
}
