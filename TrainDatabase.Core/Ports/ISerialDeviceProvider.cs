using System.Reactive;

namespace TrainDatabase.Core.Ports;

/// <summary>
/// Enumerates available serial devices (COM ports) and signals hotplug changes.
/// Replaces <c>SerialPort.GetPortNames()</c> + the <c>System.Management</c> WMI watcher.
/// Desktop enumerates real ports; Android uses USB-OTG or a stub; Browser returns empty.
/// </summary>
public interface ISerialDeviceProvider
{
    /// <summary>Returns the currently available serial port names.</summary>
    IReadOnlyList<string> GetPortNames();

    /// <summary>Emits whenever the set of available devices may have changed (hotplug).</summary>
    IObservable<Unit> DeviceChanges { get; }
}
