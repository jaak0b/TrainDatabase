using Avalonia.Threading;
using TrainDatabase.Presentation.Infrastructure;

namespace TrainDatabase.UI;

/// <summary>Marshals view-model updates onto Avalonia's UI thread.</summary>
public sealed class AvaloniaUiDispatcher : IUiDispatcher
{
    public void Post(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
        }
        else
        {
            Dispatcher.UIThread.Post(action);
        }
    }
}
