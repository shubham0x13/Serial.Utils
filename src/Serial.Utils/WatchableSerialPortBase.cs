using System;
using System.IO.Ports;

namespace Serial.Utils;

public abstract class WatchableSerialPortBase : SerialPort
{
    protected const string DefaultPortName = "COM1";

    private readonly SerialPortWatcher _watcher;
    private bool _disposed;

    public bool AutoConnect { get; set; } = false;

    public event EventHandler<SerialPortEventArgs>? PortConnected;
    public event EventHandler<SerialPortEventArgs>? PortDisconnected;
    public event EventHandler<SerialPortEventArgs>? PortOpened;
    public event EventHandler<SerialPortEventArgs>? PortClosed;

    protected WatchableSerialPortBase(
        string portName = DefaultPortName,
        int baudRate = 9600,
        Parity parity = Parity.None,
        int dataBits = 8,
        StopBits stopBits = StopBits.One)
        : base(portName, baudRate, parity, dataBits, stopBits)
    {
        _watcher = new SerialPortWatcher();
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
        catch
        {
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

    protected abstract bool ShouldHandlePortConnection(SerialPortWatcherEventArgs e);
    protected abstract bool ShouldAutoConnect();
    protected abstract bool ShouldHandlePortDisconnection(SerialPortWatcherEventArgs e);

    protected virtual void OnPortConnected() =>
        PortConnected?.Invoke(this, new SerialPortEventArgs(PortName));

    protected virtual void OnPortDisconnected() =>
        PortDisconnected?.Invoke(this, new SerialPortEventArgs(PortName));

    protected virtual void OnPortOpened() =>
        PortOpened?.Invoke(this, new SerialPortEventArgs(PortName));

    protected virtual void OnPortClosed() =>
        PortClosed?.Invoke(this, new SerialPortEventArgs(PortName));

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SinglePortWatchableSerialPort));
    }

    private void OnWatcherPortConnected(object? sender, SerialPortWatcherEventArgs e)
    {
        if (ShouldHandlePortConnection(e))
        {
            OnPortConnected();

            if (ShouldAutoConnect())
                TryConnect();
        }
    }

    private void OnWatcherPortDisconnected(object? sender, SerialPortWatcherEventArgs e)
    {
        if (ShouldHandlePortDisconnection(e))
        {
            OnPortDisconnected();
            OnPortClosed();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            Disconnect();
            _watcher.Dispose();
        }

        _disposed = true;
        base.Dispose(disposing);
    }
}
