using Reactive.Bindings;
using Z21;

namespace Core.Presenters
{
  public interface IClientPresenter
  {
    ReactiveProperty<bool> ClientReachable { get; }
  }

  public class ClientPresenter : IClientPresenter
  {
    private readonly Client client;

    public ClientPresenter(Client client)
    {
      this.client = client;

      client.ClientReachabilityChanged += (sender, clientReachable) => { ClientReachable.Value = clientReachable; };
    }

    public ReactiveProperty<bool> ClientReachable { get; } = new();
  }
}