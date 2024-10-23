using System;
using System.IO.Ports;

namespace Serial.Utils;

public class DeviceBoundSerialPort : WatchableSerialPortBase
{
    public string VendorId { get; }
    public string ProductId { get; }

    public DeviceBoundSerialPort(
        string vendorId,
        string productId,
        int baudRate = 9600,
        Parity parity = Parity.None,
        int dataBits = 8,
        StopBits stopBits = StopBits.One)
        : base(DefaultPortName, baudRate, parity, dataBits, stopBits)
    {
        if (string.IsNullOrWhiteSpace(vendorId))
            throw new ArgumentException("Vendor ID cannot be null or empty.", nameof(vendorId));

        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty.", nameof(productId));

        VendorId = vendorId;
        ProductId = productId;
    }

    public override bool TryConnect()
    {
        string[] ports = SerialPortFinder.GetPortsByVidAndPid(VendorId, ProductId);

        foreach (string port in ports)
        {
            PortName = port;
            if (base.TryConnect())
                return true;
        }

        return false;
    }

    protected override bool ShouldHandlePortConnection(SerialPortWatcherEventArgs e)
        => !IsOpen;

    protected override bool ShouldAutoConnect()
        => AutoConnect;

    protected override bool ShouldHandlePortDisconnection(SerialPortWatcherEventArgs e)
        => e.PortName == PortName;
}
