using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Ports;
using TrainDatabase.Presentation.Dialogs;
using TrainDatabase.Presentation.Files;
using TrainDatabase.Presentation.Navigation;

namespace TrainDatabase.Presentation.ViewModels;

/// <summary>Imports a Roco/Fleischmann <c>.z21</c> export, replacing the current roster.</summary>
public partial class DatabaseImportViewModel : ViewModelBase
{
    private readonly IFilePicker filePicker;
    private readonly IDatabaseImporter importer;
    private readonly IDialogService dialogs;
    private readonly VehicleTilePanelViewModel panel;
    private readonly INavigationService navigation;

    [ObservableProperty] private bool isImporting;
    [ObservableProperty] private string status = "Select a .z21 file exported from the Z21 app to import your vehicles.";

    public DatabaseImportViewModel(
        IFilePicker filePicker,
        IDatabaseImporter importer,
        IDialogService dialogs,
        VehicleTilePanelViewModel panel,
        INavigationService navigation)
    {
        this.filePicker = filePicker;
        this.importer = importer;
        this.dialogs = dialogs;
        this.panel = panel;
        this.navigation = navigation;
    }

    [RelayCommand(CanExecute = nameof(CanImport))]
    private async Task Import()
    {
        string? path = await filePicker.PickFileAsync("Select a .z21 export", new[] { "z21" });
        if (path is null)
        {
            return;
        }

        if (!await dialogs.ConfirmAsync("Import database", "This replaces all current vehicles. Continue?"))
        {
            return;
        }

        try
        {
            IsImporting = true;
            ImportCommand.NotifyCanExecuteChanged();
            Status = "Importing…";

            await importer.ImportAsync(path);

            panel.Refresh();
            Status = "Import successful.";
            await dialogs.AlertAsync("Import database", "Import successful.");
            navigation.NavigateTo(panel);
        }
        catch (Exception ex)
        {
            Status = "Import failed.";
            await dialogs.AlertAsync("Import failed", ex.Message);
        }
        finally
        {
            IsImporting = false;
            ImportCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanImport() => !IsImporting;
}
