using System.Threading.Tasks;

namespace Core.Services
{
  public interface ITrackService
  {
    /// <summary>
    /// Sets the track power to a given state.
    /// </summary>
    /// <param name="on">True sets the track power to on. False sets the track power off</param>
    Task SetTrackPowerAsync(bool on);
  }

  public class TrackService(IClientAdapter clientAdapter) : ITrackService
  {
    public async Task SetTrackPowerAsync(bool on)
    {
      await clientAdapter.SetTrackPowerAsync(on);
    }
  }
}