using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Z21;
using Z21.Model;

namespace Core.Services
{

  public class LocoSpec
  {
    public int LocoId { get; set; }

    public double MotorPower { get; set; }

    public double BrakingForce { get; set; }

    public double AdhesionCoefficient { get; set; } = 0.15;
  }

  public class TrainSpec
  {
    public string TrainId { get; set; } = Guid.NewGuid().ToString();

    public List<LocoSpec> Locos { get; set; } = new();

    public double TrainWeightTons { get; set; }

    public double TrainLengthMeters { get; set; }

    public double Throttle { get; set; } = 0.0;

    public double Brake { get; set; } = 0.0;
  }

  public class TrainState
  {
    public double Velocity { get; set; } = 0.0;

    public bool IsEmergencyBraking { get; set; } = false;
  }

  public class TrainPhysicsService
  {
    private readonly Dictionary<string, TrainSpec> trains = new();
    private readonly Dictionary<string, TrainState> states = new();
    private readonly object @lock = new();
    private readonly Client z21Client;

    public TrainPhysicsService(Client z21Client)
    {
      this.z21Client = z21Client;
    }

    public void RegisterTrain(TrainSpec train)
    {
      lock (@lock)
      {
        trains[train.TrainId] = train;
        states[train.TrainId] = new TrainState();
      }
    }

    public void UpdateTrainControl(string trainId, double throttle, double brake)
    {
      lock (@lock)
      {
        if (states.TryGetValue(trainId, out var state) && !state.IsEmergencyBraking)
        {
          var train = trains[trainId];
          train.Throttle = throttle;
          train.Brake = brake;

          if (brake >= 1.0)
            state.IsEmergencyBraking = true;
        }
      }
    }

    protected async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      const double tickRate = 0.1;

      while (!stoppingToken.IsCancellationRequested)
      {
        lock (@lock)
        {
          foreach (var (trainId, train) in trains)
          {
            var state = states[trainId];
            double massKg = train.TrainWeightTons * 1000;
            double gravity = 9.81;
            double netForce = 0;

            foreach (var loco in train.Locos)
            {
              double maxAdhesion = loco.AdhesionCoefficient * massKg * gravity;

              if (state.IsEmergencyBraking)
              {
                double braking = Math.Min(loco.BrakingForce, maxAdhesion);
                netForce -= braking;
              }
              else
              {
                double traction = loco.MotorPower * train.Throttle;
                double braking = loco.BrakingForce * train.Brake;
                double effectiveBraking = Math.Min(braking, maxAdhesion);
                netForce += traction - effectiveBraking;
              }
            }

            double acceleration = netForce / massKg;
            state.Velocity += acceleration * tickRate;

            if (state.Velocity < 0)
              state.Velocity = 0;

            if (state.IsEmergencyBraking && state.Velocity <= 0.01)
            {
              state.Velocity = 0;
              state.IsEmergencyBraking = false;
            }

            int dccSpeed = ConvertVelocityToDccSpeed(state.Velocity);

            z21Client.SetLocoDrive(train.Locos.Select(l => new LokInfoData(l.LocoId) { Speed = dccSpeed, DrivingDirection = true }).ToList());
          }
        }

        await Task.Delay(TimeSpan.FromSeconds(tickRate), stoppingToken);
      }
    }

    private int ConvertVelocityToDccSpeed(double velocity)
    {
      const double maxVelocity = 5.0;
      int speed = (int)(velocity / maxVelocity * 127);
      return Math.Clamp(speed, 0, 127);
    }
  }
}