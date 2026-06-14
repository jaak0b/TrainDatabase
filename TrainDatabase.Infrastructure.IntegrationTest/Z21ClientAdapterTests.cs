using System.Net;
using CommandStation.Model;
using CommandStation.Transport.Udp;
using Microsoft.Extensions.Logging.Abstractions;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Live;
using TrainDatabase.Infrastructure.Hardware;
using TrainDatabase.Infrastructure.IntegrationTest.Fakes;
using CoreTrackPower = TrainDatabase.Core.Live.TrackPower;

namespace TrainDatabase.Infrastructure.IntegrationTest;

[TestFixture]
public class Z21ClientAdapterTests
{
    private static Z21ClientAdapter CreateAdapter(FakeZ21CommandStation station) =>
        new(station, new UdpTransportOptions(), NullLogger<Z21ClientAdapter>.Instance);

    private static LocoInfoData Loco(ushort address, ushort speed, DrivingDirection direction, params LocoFunctionData[] functions) =>
        new()
        {
            LocoAddress = address,
            LocoSpeed = speed,
            DrivingDirection = direction,
            DccSpeedMode = DccSpeedMode.Steps128,
            DecoderMode = DecoderMode.DCC,
            LocoFunctionsData = functions,
            LocoIsBusy = false,
            LocoContainedInDoubleTraction = false,
            SmartSearch = false,
        };

    [Test]
    public void LocoInfoReceived_PublishesSpeedAndDirection()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);
        VehicleLiveData? live = null;
        adapter.VehicleData.Subscribe(data => live = data);

        station.RaiseLocoInfo(Loco(11, speed: 42, DrivingDirection.Forward));

        Assert.That(live, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(live!.VehicleAddress, Is.EqualTo(11));
            Assert.That(live.Speed, Is.EqualTo(42));
            Assert.That(live.Direction, Is.True);
        });
    }

    [Test]
    public void LocoInfoReceived_Backward_PublishesDirectionFalse()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);
        VehicleLiveData? live = null;
        adapter.VehicleData.Subscribe(data => live = data);

        station.RaiseLocoInfo(Loco(11, speed: 0, DrivingDirection.Backward));

        Assert.That(live!.Direction, Is.False);
    }

    [Test]
    public void LocoInfoReceived_PublishesFunctionStates()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);
        VehicleFunctionData? functions = null;
        adapter.VehicleFunctionData.Subscribe(data => functions = data);

        station.RaiseLocoInfo(Loco(11, 0, DrivingDirection.Forward,
            new LocoFunctionData(0, FunctionToggleType.On),
            new LocoFunctionData(3, FunctionToggleType.Off)));

        Assert.That(functions, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(functions!.VehicleAddress, Is.EqualTo(11));
            Assert.That(functions.FunctionState[0], Is.True);
            Assert.That(functions.FunctionState[3], Is.False);
        });
    }

    [Test]
    public void StatusChanged_ShortCircuit_MapsToShort()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);

        station.RaiseStatus(new CentralState { ShortCircuit = true });

        Assert.That(adapter.TrackPower.Value, Is.EqualTo(CoreTrackPower.Short));
    }

    [Test]
    public void StatusChanged_ProgrammingMode_MapsToPrograming()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);

        station.RaiseStatus(new CentralState { ProgrammingModeActive = true });

        Assert.That(adapter.TrackPower.Value, Is.EqualTo(CoreTrackPower.Programing));
    }

    [Test]
    public void StatusChanged_TrackVoltageOff_MapsToOff()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);
        station.RaiseTrackPower(true);

        station.RaiseStatus(new CentralState { TrackVoltageOff = true });

        Assert.That(adapter.TrackPower.Value, Is.EqualTo(CoreTrackPower.Off));
    }

    [Test]
    public void StatusChanged_AllClear_MapsToOn()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);

        station.RaiseStatus(new CentralState());

        Assert.That(adapter.TrackPower.Value, Is.EqualTo(CoreTrackPower.On));
    }

    [Test]
    public void TrackPowerChanged_TogglesOnAndOff()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);

        station.RaiseTrackPower(true);
        Assert.That(adapter.TrackPower.Value, Is.EqualTo(CoreTrackPower.On));

        station.RaiseTrackPower(false);
        Assert.That(adapter.TrackPower.Value, Is.EqualTo(CoreTrackPower.Off));
    }

    [Test]
    public void ConnectionChanged_UpdatesIsConnected()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);

        Assert.That(adapter.IsConnected.Value, Is.False);

        station.RaiseConnection(true);

        Assert.That(adapter.IsConnected.Value, Is.True);
    }

    [Test]
    public void Connect_ConfiguresEndpointConnectsAndPrimesStatus()
    {
        FakeZ21CommandStation station = new();
        UdpTransportOptions options = new();
        Z21ClientAdapter adapter = new(station, options, NullLogger<Z21ClientAdapter>.Instance);
        IPEndPoint endPoint = new(IPAddress.Parse("192.168.0.50"), 21105);

        adapter.Connect(endPoint);

        Assert.Multiple(() =>
        {
            Assert.That(options.RemoteEndPoint, Is.EqualTo(endPoint));
            Assert.That(station.ConnectCount, Is.EqualTo(1));
            Assert.That(station.RequestStatusCount, Is.EqualTo(1));
            Assert.That(adapter.IsConnected.Value, Is.True);
        });
    }

    [Test]
    public async Task SetVehiclesDriveAsync_WhenConnected_MapsStepDirectionAndSpeed()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);
        station.RaiseConnection(true);

        await adapter.SetVehiclesDriveAsync(new LocoSetDriveData
        {
            VehicleAddress = 11,
            Speed = 20,
            Direction = false,
            SpeedStep = RegulationStep.Step28,
        });

        DriveCall call = station.Drives.Single();
        Assert.Multiple(() =>
        {
            Assert.That(call.LocoAddress, Is.EqualTo(11));
            Assert.That(call.SpeedMode, Is.EqualTo(DccSpeedMode.Steps28));
            Assert.That(call.Direction, Is.EqualTo(DrivingDirection.Backward));
            Assert.That(call.Speed, Is.EqualTo(20));
        });
    }

    [Test]
    public async Task SetVehiclesDriveAsync_WhenDisconnected_SendsNothing()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);

        await adapter.SetVehiclesDriveAsync(new LocoSetDriveData { VehicleAddress = 11, Speed = 20 });

        Assert.That(station.Drives, Is.Empty);
    }

    [Test]
    public async Task SetVehicleFunctionAsync_WhenConnected_MapsToggleType()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);
        station.RaiseConnection(true);

        await adapter.SetVehicleFunctionAsync(vehicleAddress: 11, functionIndex: 4, on: true);
        await adapter.SetVehicleFunctionAsync(vehicleAddress: 11, functionIndex: 4, on: false);

        Assert.Multiple(() =>
        {
            Assert.That(station.Functions[0].Toggle, Is.EqualTo(FunctionToggleType.On));
            Assert.That(station.Functions[1].Toggle, Is.EqualTo(FunctionToggleType.Off));
        });
    }

    [Test]
    public async Task SetTrackPowerAsync_WhenConnected_CallsOnAndOff()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);
        station.RaiseConnection(true);

        await adapter.SetTrackPowerAsync(true);
        await adapter.SetTrackPowerAsync(false);

        Assert.That(station.TrackPowerCalls, Is.EqualTo(new[] { true, false }));
    }

    [Test]
    public async Task SetTrackPowerAsync_WhenDisconnected_SendsNothing()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);

        await adapter.SetTrackPowerAsync(true);

        Assert.That(station.TrackPowerCalls, Is.Empty);
    }

    [Test]
    public void TrackPowerChanged_DoesNotDowngradeShortCircuit()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);
        station.RaiseStatus(new CentralState { ShortCircuit = true });

        station.RaiseTrackPower(false);

        Assert.That(adapter.TrackPower.Value, Is.EqualTo(CoreTrackPower.Short));
    }

    [Test]
    public void TrackPowerChanged_DoesNotDowngradeProgrammingMode()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);
        station.RaiseStatus(new CentralState { ProgrammingModeActive = true });

        station.RaiseTrackPower(true);

        Assert.That(adapter.TrackPower.Value, Is.EqualTo(CoreTrackPower.Programing));
    }

    [Test]
    public async Task SetVehiclesDriveAsync_SpeedExceedingStepMax_IsClamped()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);
        station.RaiseConnection(true);

        await adapter.SetVehiclesDriveAsync(new LocoSetDriveData
        {
            VehicleAddress = 11,
            Speed = 100,
            Direction = true,
            SpeedStep = RegulationStep.Step28,
        });

        Assert.That(station.Drives.Single().Speed, Is.EqualTo(28));
    }

    [Test]
    public async Task SetVehiclesDriveAsync_Step128_ClampsToProtocolMaximum()
    {
        FakeZ21CommandStation station = new();
        Z21ClientAdapter adapter = CreateAdapter(station);
        station.RaiseConnection(true);

        await adapter.SetVehiclesDriveAsync(new LocoSetDriveData
        {
            VehicleAddress = 11,
            Speed = 128,
            Direction = true,
            SpeedStep = RegulationStep.Step128,
        });

        Assert.That(station.Drives.Single().Speed, Is.EqualTo(126));
    }
}
