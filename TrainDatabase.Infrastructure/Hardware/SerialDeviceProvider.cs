using System.IO.Ports;
using System.Reactive;
using System.Reactive.Subjects;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Infrastructure.Hardware;

/// <summary>
/// Enumerates serial ports via <see cref="SerialPort.GetPortNames"/>. Hotplug detection
/// (the former <c>System.Management</c> WMI watcher) is not yet re-implemented; callers can
/// re-query <see cref="GetPortNames"/> on demand. <see cref="RaiseDeviceChanged"/> lets a
/// platform-specific watcher push change notifications later.
/// </summary>
public sealed class SerialDeviceProvider : ISerialDeviceProvider, IDisposable
{
    private readonly Subject<Unit> deviceChanges = new();

    public IReadOnlyList<string> GetPortNames() => SerialPort.GetPortNames();

    public IObservable<Unit> DeviceChanges => deviceChanges;

    public void RaiseDeviceChanged() => deviceChanges.OnNext(Unit.Default);

    public void Dispose() => deviceChanges.Dispose();
}
