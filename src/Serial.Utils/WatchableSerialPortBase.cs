using System;
using System.IO.Ports;

namespace Serial.Utils;

public abstract class WatchableSerialPortBase : SerialPort
{
    private readonly SerialPortWatcher _watcher;

    private bool _disposed;

    public event EventHandler<SerialPortEventArgs>? PortConnected;
    public event EventHandler<SerialPortEventArgs>? PortDisconnected;
    public event EventHandler<SerialPortEventArgs>? PortOpened;
    public event EventHandler<SerialPortEventArgs>? PortClosed;

    public bool AutoConnect { get; set; }

    protected WatchableSerialPortBase(
        string portName = "COM1",
        int baudRate = 9600,
        Parity parity = Parity.None,
        int dataBits = 8,
        StopBits stopBits = StopBits.One,
        bool autoConnect = false)
        : base(portName, baudRate, parity, dataBits, stopBits)
    {
        AutoConnect = autoConnect;

        _watcher = new SerialPortWatcher();
        _watcher.PortConnected += OnWatcherPortConnected;
        _watcher.PortDisconnected += OnWatcherPortDisconnected;
        _watcher.Start();
    }

    public new void Open()
    {
        ThrowIfDisposed();

        base.Open();
        OnPortOpened();
    }

    public virtual bool TryOpen()
    {
        ThrowIfDisposed();

        try
        {
            base.Open();
            OnPortOpened();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public new virtual void Close()
    {
        ThrowIfDisposed();

        if (!IsOpen)
            return;

        base.Close();
        OnPortClosed();
    }

    protected abstract bool ShouldHandleConnection(SerialPortWatcherEventArgs e);

    protected abstract bool ShouldHandleDisconnection(SerialPortWatcherEventArgs e);

    protected virtual void OnPortConnected(string portName) =>
        PortConnected?.Invoke(this, new SerialPortEventArgs(portName));

    protected virtual void OnPortDisconnected() =>
        PortDisconnected?.Invoke(this, new SerialPortEventArgs(PortName));

    protected virtual void OnPortOpened() =>
        PortOpened?.Invoke(this, new SerialPortEventArgs(PortName));

    protected virtual void OnPortClosed() =>
        PortClosed?.Invoke(this, new SerialPortEventArgs(PortName));

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WatchableSerialPort));
    }

    private void OnWatcherPortConnected(object? sender, SerialPortWatcherEventArgs e)
    {
        if (ShouldHandleConnection(e))
        {
            OnPortConnected(e.PortName);

            if (AutoConnect)
                TryOpen();
        }
    }

    private void OnWatcherPortDisconnected(object? sender, SerialPortWatcherEventArgs e)
    {
        if (ShouldHandleDisconnection(e))
        {
            OnPortDisconnected();
            OnPortClosed();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Close();
            _watcher.Dispose();
        }
        
        base.Dispose(disposing);
        _disposed = true;
    }
}

public class SerialPortEventArgs : EventArgs
{
    public string PortName { get; }

    internal SerialPortEventArgs(string portName)
    {
        PortName = portName;
    }
}