using TrainDatabase.Core.Ports;
using TrainDatabase.Core.Reactive;

namespace TrainDatabase.Core.Presenters;

public interface IClientPresenter
{
    IObservableValue<bool> IsConnected { get; }

    IObservableValue<bool> IsDisconnected { get; }
}

public sealed class ClientPresenter : IClientPresenter
{
    private readonly ObservableValue<bool> isDisconnected;

    public ClientPresenter(IClientAdapter clientAdapter)
    {
        IsConnected = clientAdapter.IsConnected;
        isDisconnected = new ObservableValue<bool>(!clientAdapter.IsConnected.Value);
        clientAdapter.IsConnected.Subscribe(connected => isDisconnected.SetValue(!connected));
        IsDisconnected = isDisconnected;
    }

    public IObservableValue<bool> IsConnected { get; }

    public IObservableValue<bool> IsDisconnected { get; }
}
