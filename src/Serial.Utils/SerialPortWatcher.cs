using System;
using System.Management;

namespace Serial.Utils;

public class SerialPortWatcher : IDisposable
{
    public bool IsWatching { get; private set; }

    private readonly ManagementEventWatcher _connectEventWatcher;
    private readonly ManagementEventWatcher _disconnectEventWatcher;

    public event EventHandler<SerialPortWatcherEventArgs>? PortConnected;
    public event EventHandler<SerialPortWatcherEventArgs>? PortDisconnected;
    private bool _disposed;

    public SerialPortWatcher(int pollingInterval = 1)
    {
        if (pollingInterval < 1)
            throw new ArgumentOutOfRangeException(nameof(pollingInterval), "Polling interval must be greater than 0.");

        _connectEventWatcher = CreateWatcher("Creation", pollingInterval, OnPortConnected);
        _disconnectEventWatcher = CreateWatcher("Deletion", pollingInterval, OnPortDisconnected);
    }

    public void Start()
    {
        ThrowIfDisposed();

        if (IsWatching)
            throw new InvalidOperationException("Already watching.");

        _connectEventWatcher.Start();
        _disconnectEventWatcher.Start();
        IsWatching = true;
    }

    public void Stop()
    {
        ThrowIfDisposed();

        if (!IsWatching)
            return;

        _connectEventWatcher.Stop();
        _disconnectEventWatcher.Stop();
        IsWatching = false;
    }

    private ManagementEventWatcher CreateWatcher(string eventType, int pollingInterval, EventArrivedEventHandler handler)
    {
        string query = $"SELECT * FROM __Instance{eventType}Event WITHIN {pollingInterval} WHERE TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.Name LIKE '%(COM%'";
        ManagementEventWatcher watcher = new ManagementEventWatcher(new WqlEventQuery(query));
        watcher.EventArrived += handler;
        return watcher;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SerialPortWatcher));
    }

    private void OnPortConnected(object sender, EventArrivedEventArgs e)
    {
        PortConnected?.Invoke(this, new SerialPortWatcherEventArgs(e.NewEvent));
    }

    private void OnPortDisconnected(object sender, EventArrivedEventArgs e)
    {
        PortDisconnected?.Invoke(this, new SerialPortWatcherEventArgs(e.NewEvent));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _connectEventWatcher.Stop();
        _connectEventWatcher.Dispose();

        _disconnectEventWatcher.Stop();
        _disconnectEventWatcher.Dispose();

        IsWatching = false;

        _disposed = true;
    }
}

public class SerialPortWatcherEventArgs : EventArgs
{
    public string DeviceID { get; }
    public string Name { get; }
    public string Caption { get; }
    public string Description { get; }
    public string ClassGuid { get; }
    public string PortName { get; }

    internal SerialPortWatcherEventArgs(ManagementBaseObject eventObject)
    {
        ManagementBaseObject targetInstance = (ManagementBaseObject)eventObject["TargetInstance"];

        DeviceID = GetPropertyValue(targetInstance, "DeviceID");
        Name = GetPropertyValue(targetInstance, "Name");
        Caption = GetPropertyValue(targetInstance, "Caption");
        Description = GetPropertyValue(targetInstance, "Description");
        ClassGuid = GetPropertyValue(targetInstance, "ClassGuid");

        PortName = SerialPortUtils.ExtractPortNameFromDeviceName(Name) ?? string.Empty;
    }

    private string GetPropertyValue(ManagementBaseObject targetInstance, string propertyName)
        => targetInstance[propertyName]?.ToString() ?? string.Empty;
}
