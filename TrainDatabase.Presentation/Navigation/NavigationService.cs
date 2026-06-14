namespace TrainDatabase.Presentation.Navigation;

/// <summary>Default <see cref="INavigationService"/> backed by an in-memory back stack.</summary>
public sealed class NavigationService : INavigationService
{
    private readonly Stack<ViewModelBase> backStack = new();

    public ViewModelBase? Current { get; private set; }

    public bool CanGoBack => backStack.Count > 0;

    public event EventHandler? CurrentChanged;

    public void NavigateTo(ViewModelBase viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        if (Current is not null)
        {
            backStack.Push(Current);
        }

        Current = viewModel;
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Back()
    {
        if (!CanGoBack)
        {
            return;
        }

        Current = backStack.Pop();
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }
}
