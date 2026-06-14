namespace TrainDatabase.Presentation.Infrastructure;

/// <summary>
/// Marshals an action onto the UI thread. The UI head supplies an Avalonia-backed
/// implementation; tests use a synchronous one.
/// </summary>
public interface IUiDispatcher
{
    void Post(Action action);
}

/// <summary>Runs the action immediately on the calling thread (for tests / non-UI contexts).</summary>
public sealed class ImmediateUiDispatcher : IUiDispatcher
{
    public void Post(Action action) => action();
}
