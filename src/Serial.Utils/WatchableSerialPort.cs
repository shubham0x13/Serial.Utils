using System.IO.Ports;

namespace Serial.Utils;

public class WatchableSerialPort : WatchableSerialPortBase
{
    public WatchableSerialPort(
        string portName = "COM1",
        int baudRate = 9600,
        Parity parity = Parity.None,
        int dataBits = 8,
        StopBits stopBits = StopBits.One,
        bool autoConnect = false)
        : base(portName, baudRate, parity, dataBits, stopBits, autoConnect)
    {
    }

    protected override bool ShouldHandleConnection(SerialPortWatcherEventArgs e) => e.PortName == PortName;

    protected override bool ShouldHandleDisconnection(SerialPortWatcherEventArgs e) => e.PortName == PortName;
}
