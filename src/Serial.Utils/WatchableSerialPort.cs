using System;
using System.Diagnostics;
using System.IO.Ports;

namespace Serial.Utils;

public class WatchableSerialPort : SerialPort
{
    public bool AutoConnect { get; set; } = false;

    public event EventHandler<SerialPortEventArgs>? PortConnected;
    public event EventHandler<SerialPortEventArgs>? PortDisconnected;

    public event EventHandler<SerialPortEventArgs>? PortOpened;
    public event EventHandler<SerialPortEventArgs>? PortClosed;

    private readonly SerialPortWatcher _watcher = new SerialPortWatcher();
    private bool _disposed;

    public WatchableSerialPort() : this("COM1") { }

    public WatchableSerialPort(string portName) : this(portName, 9600) { }

    public WatchableSerialPort(string portName, int baudRate) : this(portName, baudRate, Parity.None) { }

    public WatchableSerialPort(string portName, int baudRate, Parity parity) : this(portName, baudRate, parity, 8) { }

    public WatchableSerialPort(string portName, int baudRate, Parity parity, int dataBits) : this(portName, baudRate, parity, dataBits, StopBits.One) { }

    public WatchableSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        : base(portName, baudRate, parity, dataBits, stopBits)
    {
        _watcher.PortConnected += OnWatcherPortConnected;
        _watcher.PortDisconnected += OnWatcherPortDisconnected;
        _watcher.Start();
    }

    [Obsolete("Use TryConnect() instead.", true)]
    public new void Open() => throw new NotImplementedException();

    [Obsolete("Use Disconnect() instead.", true)]
    public new void Close() => throw new NotImplementedException();

    public virtual bool TryConnect()
    {
        ThrowIfDisposed();

        try
        {
            base.Open();
            OnPortOpened();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return false;
        }
    }

    public virtual void Disconnect()
    {
        ThrowIfDisposed();

        if (!IsOpen)
            return;

        base.Close();
        OnPortClosed();
    }

    protected virtual void OnPortConnected() => PortConnected?.Invoke(this, new SerialPortEventArgs(PortName));

    protected virtual void OnPortDisconnected() => PortDisconnected?.Invoke(this, new SerialPortEventArgs(PortName));

    protected virtual void OnPortOpened() => PortOpened?.Invoke(this, new SerialPortEventArgs(PortName));

    protected virtual void OnPortClosed() => PortClosed?.Invoke(this, new SerialPortEventArgs(PortName));

    protected virtual void OnWatcherPortConnected(object sender, SerialPortWatcherEventArgs e)
    {
        if (e.PortName == PortName)
        {
            OnPortConnected();

            if (AutoConnect)
                TryConnect();
        }
    }

    protected virtual void OnWatcherPortDisconnected(object sender, SerialPortWatcherEventArgs e)
    {
        if (e.PortName == PortName)
        {
            OnPortDisconnected();
            OnPortClosed();
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WatchableSerialPort));
    }

    protected override void Dispose(bool disposing)
    {
        ThrowIfDisposed();

        if (disposing)
        {
            Disconnect();
            _watcher.Dispose();

            PortConnected = null;
            PortDisconnected = null;
            PortOpened = null;
            PortClosed = null;
        }

        _disposed = true;
        base.Dispose(disposing);
    }
}

public class SerialPortEventArgs : EventArgs
{
    public string PortName { get; }

    internal SerialPortEventArgs(string portName)
    {
        PortName = portName ?? throw new ArgumentNullException(nameof(portName));
    }
}
