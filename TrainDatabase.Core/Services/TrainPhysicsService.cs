using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Live;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Core.Services;

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

    public double Throttle { get; set; }

    public double Brake { get; set; }
}

public class TrainState
{
    public double Velocity { get; set; }

    public bool IsEmergencyBraking { get; set; }
}

/// <summary>
/// Experimental physics-based throttle model that converts simulated velocity into DCC
/// speed steps and drives the locos through the command station abstraction.
/// </summary>
public class TrainPhysicsService(IClientAdapter clientAdapter)
{
    private readonly Dictionary<string, TrainSpec> trains = new();
    private readonly Dictionary<string, TrainState> states = new();
    private readonly object @lock = new();

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
            if (states.TryGetValue(trainId, out TrainState? state) && !state.IsEmergencyBraking)
            {
                TrainSpec train = trains[trainId];
                train.Throttle = throttle;
                train.Brake = brake;

                if (brake >= 1.0)
                {
                    state.IsEmergencyBraking = true;
                }
            }
        }
    }

    public static int ConvertVelocityToDccSpeed(double velocity)
    {
        const double maxVelocity = 5.0;
        int speed = (int)(velocity / maxVelocity * DccConstants.MaxDccStep);
        return Math.Clamp(speed, 0, DccConstants.MaxDccStep);
    }

    protected async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const double tickRate = 0.1;
        const double gravity = 9.81;

        while (!stoppingToken.IsCancellationRequested)
        {
            List<LocoSetDriveData> commands = new();
            lock (@lock)
            {
                foreach ((string trainId, TrainSpec train) in trains)
                {
                    TrainState state = states[trainId];
                    double massKg = train.TrainWeightTons * 1000;
                    double netForce = 0;

                    foreach (LocoSpec loco in train.Locos)
                    {
                        double maxAdhesion = loco.AdhesionCoefficient * massKg * gravity;
                        if (state.IsEmergencyBraking)
                        {
                            netForce -= Math.Min(loco.BrakingForce, maxAdhesion);
                        }
                        else
                        {
                            double traction = loco.MotorPower * train.Throttle;
                            double braking = Math.Min(loco.BrakingForce * train.Brake, maxAdhesion);
                            netForce += traction - braking;
                        }
                    }

                    double acceleration = massKg > 0 ? netForce / massKg : 0;
                    state.Velocity = Math.Max(0, state.Velocity + acceleration * tickRate);

                    if (state.IsEmergencyBraking && state.Velocity <= 0.01)
                    {
                        state.Velocity = 0;
                        state.IsEmergencyBraking = false;
                    }

                    int dccSpeed = ConvertVelocityToDccSpeed(state.Velocity);
                    commands.AddRange(train.Locos.Select(l => new LocoSetDriveData
                    {
                        VehicleAddress = (ushort)l.LocoId,
                        Speed = (ushort)dccSpeed,
                        Direction = true,
                    }));
                }
            }

            if (commands.Count > 0)
            {
                await clientAdapter.SetVehiclesDriveAsync(commands.ToArray());
            }

            await Task.Delay(TimeSpan.FromSeconds(tickRate), stoppingToken);
        }
    }
}
