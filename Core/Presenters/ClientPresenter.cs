using Reactive.Bindings;

namespace Core.Presenters
{
  public interface IClientPresenter
  {
    ReadOnlyReactiveProperty<bool> IsConnected { get; }
  }

  public class ClientPresenter : IClientPresenter
  {

    public ClientPresenter(IClientAdapter clientAdapter)
    {
      IsConnected = clientAdapter.IsConnected.ToReadOnlyReactiveProperty();
    }

    public ReadOnlyReactiveProperty<bool> IsConnected { get; }
  }
}