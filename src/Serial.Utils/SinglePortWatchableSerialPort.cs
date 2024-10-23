using System;
using System.IO.Ports;

namespace Serial.Utils;

public class SinglePortWatchableSerialPort : WatchableSerialPortBase
{
    public SinglePortWatchableSerialPort(
        string portName = DefaultPortName,
        int baudRate = 9600,
        Parity parity = Parity.None,
        int dataBits = 8,
        StopBits stopBits = StopBits.One)
        : base(portName, baudRate, parity, dataBits, stopBits)
    {
    }

    protected override bool ShouldHandlePortConnection(SerialPortWatcherEventArgs e)
        => e.PortName == PortName;

    protected override bool ShouldAutoConnect()
        => AutoConnect;

    protected override bool ShouldHandlePortDisconnection(SerialPortWatcherEventArgs e)
        => e.PortName == PortName;
}

public class SerialPortEventArgs : EventArgs
{
    public string PortName { get; }

    internal SerialPortEventArgs(string portName)
    {
        PortName = portName ?? throw new ArgumentNullException(nameof(portName));
    }
}
