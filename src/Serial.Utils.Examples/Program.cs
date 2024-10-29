namespace Serial.Utils.Examples;

internal class Program
{
    static void Main(string[] args)
    {
        DeviceBoundSerialPort serialPort = new DeviceBoundSerialPort("4D43", "4D50", 115200, autoConnect: true);

        serialPort.PortConnected += (sender, e) =>
        {
            Console.WriteLine($"Port {e.PortName} connected.");
        };

        serialPort.PortDisconnected += (sender, e) =>
        {
            Console.WriteLine($"Port {e.PortName} disconnected.");
        };

        serialPort.PortOpened += (sender, e) =>
        {
            Console.WriteLine($"Port {e.PortName} opened.");
        };

        serialPort.PortClosed += (sender, e) =>
        {
            Console.WriteLine($"Port {e.PortName} closed.");
        };

        bool connected = serialPort.TryOpen();
        Console.WriteLine($"Connected: {connected}");

        Console.ReadKey(true);
    }
}
