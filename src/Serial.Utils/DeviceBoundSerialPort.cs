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
        StopBits stopBits = StopBits.One,
        bool autoConnect = false)
        : base("COM1", baudRate, parity, dataBits, stopBits, autoConnect)
    {
        if (string.IsNullOrWhiteSpace(vendorId))
            throw new ArgumentException("Vendor ID cannot be null or empty.", nameof(vendorId));

        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty.", nameof(productId));

        VendorId = vendorId;
        ProductId = productId;
    }

    public override bool TryOpen()
    {
        foreach (string port in SerialPortFinder.GetPortsByVidAndPid(VendorId, ProductId))
        {
            PortName = port;
            if (base.TryOpen())
                return true;
        }

        return false;
    }

    protected override bool ShouldHandleConnection(SerialPortWatcherEventArgs e) => !IsOpen;

    protected override bool ShouldHandleDisconnection(SerialPortWatcherEventArgs e) => e.PortName == PortName;
}
