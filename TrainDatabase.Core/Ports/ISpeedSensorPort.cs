namespace TrainDatabase.Core.Ports;

/// <summary>
/// Factory for a speed-sensor connection (today: an Arduino over a serial port).
/// </summary>
public delegate ISpeedSensorPort SpeedSensorPortFactory(string portName, int baudRate = 9600);

/// <summary>
/// A connection to the speed-measurement sensor. Implemented in Infrastructure on
/// platforms that support the underlying transport; stubbed on the Browser head.
/// </summary>
public interface ISpeedSensorPort : IDisposable
{
    /// <summary>
    /// Reads the measured duration (in milliseconds) between the two sensors.
    /// </summary>
    /// <param name="timeout"><see cref="TimeSpan"/> after which <c>null</c> is returned.</param>
    /// <returns>The duration in milliseconds, or <c>null</c> if the timeout elapsed.</returns>
    Task<decimal?> ReadDurationAsync(TimeSpan timeout);
}
