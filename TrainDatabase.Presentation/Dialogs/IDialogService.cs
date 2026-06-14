namespace TrainDatabase.Presentation.Dialogs;

/// <summary>
/// Cross-platform dialogs rendered as an overlay in the shell. Replaces WPF
/// <c>MessageBox</c>/<c>ShowDialog</c> with async overlay calls.
/// </summary>
public interface IDialogService
{
    Task AlertAsync(string title, string message);

    Task<bool> ConfirmAsync(string title, string message);
}
