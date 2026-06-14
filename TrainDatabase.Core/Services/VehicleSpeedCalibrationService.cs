using Microsoft.Extensions.Logging;
using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Core.Reactive;

namespace TrainDatabase.Core.Services;

public interface IVehicleSpeedCalibrationService
{
    IObservableValue<ServiceState> ServiceState { get; }

    Task CalibrateVehicleAsync(Vehicle vehicle);
}

/// <summary>
/// Drives a vehicle through its speed steps while reading the speed sensor, recording the
/// measured real-world speed per step/direction as <see cref="VehicleCalibrationData"/>.
/// </summary>
public class VehicleSpeedCalibrationService(
    ITrackPresenter trackPresenter,
    IVehicleControlService vehicleControlService,
    VehiclePresenterFactory vehiclePresenterFactory,
    SpeedSensorPortFactory speedSensorPortFactory,
    ILogger<VehicleSpeedCalibrationService> logger) : IVehicleSpeedCalibrationService
{
    private readonly ObservableValue<ServiceState> serviceState = new(Services.ServiceState.Idle());

    public IObservableValue<ServiceState> ServiceState => serviceState;

    public async Task CalibrateVehicleAsync(Vehicle vehicle)
    {
        if (trackPresenter.TrackPower.Value != TrackPower.On)
        {
            throw new InvalidOperationException("Track power is off. It must be on to calibrate.");
        }

        try
        {
            serviceState.SetValue(Services.ServiceState.Running(vehicle.Id));
            vehicle.VehicleCalibrations.Clear();

            IVehiclePresenter vehiclePresenter = vehiclePresenterFactory(vehicle.Id);
            int maximumSpeedStep = vehiclePresenter.MaximumSpeedStep.Value;

            const int startStep = 1;
            const int incrementStepBy = 5;
            const decimal sensorDistanceMm = 200;
            string portName = "";
            const int baudRate = 9600;

            bool lastStep = false;
            using ISpeedSensorPort sensor = speedSensorPortFactory(portName, baudRate);
            for (int speed = startStep; speed <= maximumSpeedStep; speed += incrementStepBy)
            {
                await CaptureTime(sensor, vehicle, speed, true, sensorDistanceMm);
                await CaptureTime(sensor, vehicle, speed, false, sensorDistanceMm);

                if (!lastStep && speed + incrementStepBy > maximumSpeedStep)
                {
                    speed = maximumSpeedStep - incrementStepBy;
                    lastStep = true;
                }
            }

            await ReturnToStartPosition(sensor, vehicle);
        }
        finally
        {
            serviceState.SetValue(Services.ServiceState.Idle());
        }
    }

    private async Task CaptureTime(ISpeedSensorPort sensor, Vehicle vehicle, int speedStep, bool direction, decimal sensorDistanceMm)
    {
        try
        {
            await vehicleControlService.SetVehicleSpeedAsync(vehicle, speedStep, direction);

            decimal timeRaw = await sensor.ReadDurationAsync(TimeSpan.FromMinutes(5))
                ?? throw new ApplicationException("No data received from the speed sensor.");

            decimal time = timeRaw / 1000.0m;
            // 87 = H0 scale factor; should become configurable for other scales.
            decimal speed = Math.Round(sensorDistanceMm / 1000.0m / time * 87.0m, 2);

            vehicle.VehicleCalibrations.Add(new VehicleCalibrationData
            {
                Vehicle = vehicle,
                VehicleId = vehicle.Id,
                Direction = direction,
                SpeedStep = speedStep,
                MeasuredSpeed = speed,
            });

            logger.LogInformation("Vehicle {VehicleId} measured {Speed} at step {Step} ({Direction}).",
                vehicle.Id, speed, speedStep, direction);
        }
        finally
        {
            await vehicleControlService.SetVehicleSpeedAsync(vehicle, 0, direction);
        }
    }

    private async Task ReturnToStartPosition(ISpeedSensorPort sensor, Vehicle vehicle)
    {
        await vehicleControlService.SetVehicleSpeedAsync(vehicle, 40, true);
        await sensor.ReadDurationAsync(TimeSpan.FromMinutes(5));
        await vehicleControlService.SetVehicleSpeedAsync(vehicle, 40, false);
        await sensor.ReadDurationAsync(TimeSpan.FromMinutes(5));
        await vehicleControlService.SetVehicleSpeedAsync(vehicle, 0, true);
    }
}
