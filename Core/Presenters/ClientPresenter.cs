using System.Reactive.Linq;
using Reactive.Bindings;

namespace Core.Presenters
{
  public interface IClientPresenter
  {
    ReadOnlyReactiveProperty<bool> IsConnected { get; }

    ReadOnlyReactiveProperty<bool> IsDisconnected { get; }
  }

  public class ClientPresenter : IClientPresenter
  {

    public ClientPresenter(IClientAdapter clientAdapter)
    {
      IsConnected = clientAdapter.IsConnected.ToReadOnlyReactiveProperty();
      IsDisconnected = clientAdapter.IsConnected
                                    .Select(isConnected => !isConnected)
                                    .ToReadOnlyReactiveProperty();
    }

    public ReadOnlyReactiveProperty<bool> IsConnected { get; }

    public ReadOnlyReactiveProperty<bool> IsDisconnected { get; }
  }
}