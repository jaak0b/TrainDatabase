using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using TrainDatabase.Presentation.Files;

namespace TrainDatabase.UI;

/// <summary>Avalonia <see cref="IFilePicker"/> over the active top-level's StorageProvider.</summary>
public sealed class AvaloniaFilePicker : IFilePicker
{
    public async Task<string?> PickFileAsync(string title, IReadOnlyList<string> extensions)
    {
        TopLevel? topLevel = GetTopLevel();
        if (topLevel is null)
        {
            return null;
        }

        FilePickerFileType fileType = new("Import file")
        {
            Patterns = extensions.Select(extension => $"*.{extension}").ToList(),
        };

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = new[] { fileType },
        });

        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    private static TopLevel? GetTopLevel() => Application.Current?.ApplicationLifetime switch
    {
        IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
        ISingleViewApplicationLifetime singleView => TopLevel.GetTopLevel(singleView.MainView),
        _ => null,
    };
}
