namespace TrainDatabase.Presentation.Navigation;

/// <summary>
/// ViewModel-first router for the single-page shell. Navigating sets <see cref="Current"/>,
/// which the shell view renders via a ViewLocator. Maintains a back stack.
/// </summary>
public interface INavigationService
{
    ViewModelBase? Current { get; }

    bool CanGoBack { get; }

    event EventHandler? CurrentChanged;

    void NavigateTo(ViewModelBase viewModel);

    void Back();
}
