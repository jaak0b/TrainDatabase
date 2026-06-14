namespace TrainDatabase.Presentation.Files;

/// <summary>Picks a file from the platform's file system. Implemented per UI head.</summary>
public interface IFilePicker
{
    /// <summary>
    /// Shows a file-open dialog filtered to the given extensions (without the dot, e.g. "z21").
    /// Returns the chosen local path, or <c>null</c> if cancelled / unavailable.
    /// </summary>
    Task<string?> PickFileAsync(string title, IReadOnlyList<string> extensions);
}
