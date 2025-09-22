using Core.Model;
using Reactive.Bindings;

namespace Core.Presenters
{
  public interface ITrackPresenter
  {
    ReadOnlyReactiveProperty<TrackPower> TrackPower { get; }
  }

  public class TrackPresenter : ITrackPresenter
  {
    public TrackPresenter(IClientAdapter clientAdapter)
    {
      TrackPower = clientAdapter.TrackPower.ToReadOnlyReactiveProperty();
    }

    public ReadOnlyReactiveProperty<TrackPower> TrackPower { get; }
  }
}