using TrainDatabase.Core.Live;
using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Reactive;

namespace TrainDatabase.Core.Presenters;

public interface ITrackPresenter
{
    IObservableValue<TrackPower> TrackPower { get; }
}

public sealed class TrackPresenter(IClientAdapter clientAdapter) : ITrackPresenter
{
    public IObservableValue<TrackPower> TrackPower { get; } = clientAdapter.TrackPower;
}
