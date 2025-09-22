using System;
using System.Threading.Tasks;

namespace Core
{
  public delegate ISpeedSensorPort SpeedSensorPortFactory(string portName, int baudRate = 9600);

  public interface ISpeedSensorPort : IDisposable
  {
    /// <summary>
    /// Reads the measured value in millisecond.
    /// </summary>
    /// <param name="timeout"><see cref="TimeSpan"/> after witch null will be returned.</param>
    /// <returns>Returns the duration in milliseconds or null if the <paramref name="timeout"/> was exceeded.</returns>
    public Task<decimal?> ReadDurationAsync(TimeSpan timeout);
  }
}