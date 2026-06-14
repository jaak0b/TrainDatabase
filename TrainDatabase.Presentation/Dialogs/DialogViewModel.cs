using CommunityToolkit.Mvvm.Input;

namespace TrainDatabase.Presentation.Dialogs;

/// <summary>A single overlay dialog (alert or confirm) awaiting a boolean result.</summary>
public sealed partial class DialogViewModel : ViewModelBase
{
    private readonly TaskCompletionSource<bool> completion = new();

    public DialogViewModel(string title, string message, bool showCancel)
    {
        Title = title;
        Message = message;
        ShowCancel = showCancel;
    }

    public string Title { get; }

    public string Message { get; }

    public bool ShowCancel { get; }

    public Task<bool> Result => completion.Task;

    [RelayCommand]
    private void Accept() => completion.TrySetResult(true);

    [RelayCommand]
    private void Cancel() => completion.TrySetResult(false);
}
