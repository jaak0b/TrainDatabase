namespace TrainDatabase.Presentation.Dialogs;

/// <summary>
/// Default <see cref="IDialogService"/>. Holds the <see cref="Current"/> dialog the shell
/// renders in its overlay; awaiting the dialog's result clears it.
/// </summary>
public sealed class DialogService : IDialogService
{
    public DialogViewModel? Current { get; private set; }

    public event EventHandler? CurrentChanged;

    public Task AlertAsync(string title, string message) => ShowAsync(new DialogViewModel(title, message, showCancel: false));

    public Task<bool> ConfirmAsync(string title, string message) => ShowAsync(new DialogViewModel(title, message, showCancel: true));

    private async Task<bool> ShowAsync(DialogViewModel dialog)
    {
        Current = dialog;
        CurrentChanged?.Invoke(this, EventArgs.Empty);

        bool result = await dialog.Result;

        Current = null;
        CurrentChanged?.Invoke(this, EventArgs.Empty);
        return result;
    }
}
