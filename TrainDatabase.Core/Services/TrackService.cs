using TrainDatabase.Core.Ports;

namespace TrainDatabase.Core.Services;

public interface ITrackService
{
    /// <summary>Sets the track power. True turns power on, false turns it off.</summary>
    Task SetTrackPowerAsync(bool on);
}

public class TrackService(IClientAdapter clientAdapter) : ITrackService
{
    public Task SetTrackPowerAsync(bool on) => clientAdapter.SetTrackPowerAsync(on);
}
