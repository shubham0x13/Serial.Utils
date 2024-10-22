using System;
using System.IO.Ports;

namespace Serial.Utils;

public class DeviceBoundSerialPort : WatchableSerialPort
{
    public string VendorId { get; }
    public string ProductId { get; }

    public DeviceBoundSerialPort(string vendorId, string productId) : this(vendorId, productId, 9600) { }

    public DeviceBoundSerialPort(string vendorId, string productId, int baudRate) : this(vendorId, productId, baudRate, Parity.None) { }

    public DeviceBoundSerialPort(string vendorId, string productId, int baudRate, Parity parity) : this(vendorId, productId, baudRate, parity, 8, StopBits.One) { }

    public DeviceBoundSerialPort(string vendorId, string productId, int baudRate, Parity parity, int dataBits) : this(vendorId, productId, baudRate, parity, dataBits, StopBits.One) { }

    public DeviceBoundSerialPort(string vendorId, string productId, int baudRate, Parity parity, int dataBits, StopBits stopBits) : base("COM1", baudRate, parity, dataBits, stopBits)
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

    protected override void OnWatcherPortConnected(object sender, SerialPortWatcherEventArgs e)
    {
        if (!IsOpen)
        {
            OnPortConnected();

            if (AutoConnect)
                TryConnect();
        }
    }
}
