using System.Net;
using System.Reactive.Subjects;
using Autofac;
using Avalonia;
using Avalonia.Headless;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Reactive;
using TrainDatabase.Infrastructure.Platform;
using TrainDatabase.UI;
using TrainDatabase.UI.EndToEndTest;

[assembly: Avalonia.Headless.AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace TrainDatabase.UI.EndToEndTest;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

/// <summary>Overrides platform storage (to a temp folder) and the command station (fake) for E2E.</summary>
public sealed class TestOverrideModule(string baseDirectory, IClientAdapter clientAdapter) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(new DesktopAppStorage(baseDirectory)).As<IAppStorage>().SingleInstance();
        builder.RegisterInstance(clientAdapter).As<IClientAdapter>().SingleInstance();
    }
}

public sealed class FakeClientAdapter : IClientAdapter
{
    public Subject<VehicleLiveData> VehicleDataSubject { get; } = new();
    public ObservableValue<bool> IsConnectedValue { get; } = new(true);
    public ObservableValue<TrackPower> TrackPowerValue { get; } = new(TrainDatabase.Core.Live.TrackPower.On);
    public List<LocoSetDriveData> DriveCommands { get; } = new();

    public void Connect(IPEndPoint endPoint) { }
    public IObservable<VehicleLiveData> VehicleData => VehicleDataSubject;
    public IObservable<VehicleFunctionData> VehicleFunctionData { get; } = new Subject<VehicleFunctionData>();
    public IObservableValue<bool> IsConnected => IsConnectedValue;
    public IObservableValue<TrackPower> TrackPower => TrackPowerValue;

    public Task SetVehiclesDriveAsync(params LocoSetDriveData[] locoSetDriveDatas)
    {
        DriveCommands.AddRange(locoSetDriveDatas);
        return Task.CompletedTask;
    }

    public List<(ushort Address, ushort Function, bool On)> FunctionCommands { get; } = new();
    public Task SetVehicleFunctionAsync(ushort vehicleAddress, ushort functionIndex, bool on)
    {
        FunctionCommands.Add((vehicleAddress, functionIndex, on));
        return Task.CompletedTask;
    }

    public Task SetTrackPowerAsync(bool on) => Task.CompletedTask;
}
