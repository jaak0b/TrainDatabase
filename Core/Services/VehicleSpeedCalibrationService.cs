using System;
using System.Threading.Tasks;
using Core.Model;
using Core.Presenters;
using Microsoft.Extensions.Logging;
using Persistence.Model;
using Reactive.Bindings;

namespace Core.Services
{
  public interface IVehicleSpeedCalibrationService
  {
    Task CalibrateVehicleAsync(Vehicle vehicle);
  }

  public class VehicleSpeedCalibrationService(ITrackPresenter trackPresenter,
                                              IVehicleControlService vehicleControlService,
                                              VehiclePresenterFactory vehiclePresenterFactory,
                                              SpeedSensorPortFactory speedSensorPortFactory,
                                              ILogger<VehicleSpeedCalibrationService> logger) : IVehicleSpeedCalibrationService
  {
    public ReactiveProperty<ServiceState> ServiceState { get; private set; } = new();

    public async Task CalibrateVehicleAsync(Vehicle vehicle)
    {
      try
      {
        if (trackPresenter.TrackPower.Value != TrackPower.On)
          throw new InvalidOperationException("Track Power Off. Must be on.");

        ServiceState.Value = Core.Model.ServiceState.Running(vehicle.Id);

        vehicle.VehicleCalibrations.Clear();

        IVehiclePresenter vehiclePresenter = vehiclePresenterFactory(vehicle.Id);
        int maximumSpeedStep = vehiclePresenter.MaximumSpeedStep.Value;

        bool lastStep = false;

        int startStep = 1; // make it configurable
        int incrementStepBy = 5;
        decimal sensorDistance = 200; // in mm
        string portName = "";
        int baudRate = 9600;
        
        using ISpeedSensorPort sensor = speedSensorPortFactory(portName, baudRate);
        for (int speed = startStep; speed <= maximumSpeedStep; speed += incrementStepBy)
        {
          await CaptureTime(sensor, vehicle, speed, true, sensorDistance);
          await CaptureTime(sensor, vehicle, speed, false, sensorDistance);

          if (!lastStep && speed + incrementStepBy > maximumSpeedStep)
          {
            speed = maximumSpeedStep - incrementStepBy;
            lastStep = true;
          }
        }

        await ReturnToStartPosition(sensor, vehicle);
      } finally
      {
        ServiceState.Value = Core.Model.ServiceState.Idle();
      }
    }

    private async Task CaptureTime(ISpeedSensorPort sensor, Vehicle vehicle, int speedStep, bool direction, decimal sensorDistance)
    {
      try
      {
        await vehicleControlService.SetVehicleSpeedAsync(vehicle, speedStep, direction);

        decimal timeRaw = await sensor.ReadDurationAsync(TimeSpan.FromMinutes(5)) ?? throw new ApplicationException($"Na data on serial");

        decimal time = timeRaw / 1000.0m;
        decimal speed = Math.Round(sensorDistance / 1000.0m / time * 87.0m, 2); // 87 should be configurable to allow for different train sizes then h0 scale.

        VehicleCalibrationData calibrationData = new()
                                                 {
                                                   Vehicle = vehicle,
                                                   VehicleId = vehicle.Id,
                                                   Direction = direction,
                                                   SpeedStep = speedStep,
                                                   MeasuredSpeed = speed,
                                                 };
        vehicle.VehicleCalibrations.Add(calibrationData);
      } finally
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
}