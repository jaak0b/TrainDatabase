using TrainDatabase.Core.Ports;
using TrainDatabase.Presentation.Navigation;
using TrainDatabase.Presentation.UnitTest.Fakes;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class DatabaseImportViewModelTests
{
    private static (DatabaseImportViewModel Vm, FakeDatabaseImporter Importer, NavigationService Nav, VehicleTilePanelViewModel Panel)
        Build(string? pickedPath, bool confirm)
    {
        FakeVehicleRepository repository = new();
        VehicleTileViewModelFactory tileFactory = _ => throw new InvalidOperationException("no tiles expected");
        VehicleTilePanelViewModel panel = new(repository, tileFactory);
        NavigationService navigation = new();
        FakeDatabaseImporter importer = new();
        FakeDialogService dialogs = new() { ConfirmResult = confirm };

        DatabaseImportViewModel vm = new(new FakeFilePicker(pickedPath), importer, dialogs, panel, navigation);
        return (vm, importer, navigation, panel);
    }

    [Test]
    public async Task Import_WhenFileChosenAndConfirmed_RunsImportAndNavigatesToPanel()
    {
        (DatabaseImportViewModel vm, FakeDatabaseImporter importer, NavigationService nav, VehicleTilePanelViewModel panel) =
            Build(pickedPath: @"C:\layouts\roster.z21", confirm: true);

        await vm.ImportCommand.ExecuteAsync(null);

        Assert.Multiple(() =>
        {
            Assert.That(importer.Imported, Is.EqualTo(new[] { @"C:\layouts\roster.z21" }));
            Assert.That(nav.Current, Is.SameAs(panel));
            Assert.That(vm.IsImporting, Is.False);
        });
    }

    [Test]
    public async Task Import_WhenCancelledAtFilePicker_DoesNothing()
    {
        (DatabaseImportViewModel vm, FakeDatabaseImporter importer, _, _) = Build(pickedPath: null, confirm: true);

        await vm.ImportCommand.ExecuteAsync(null);

        Assert.That(importer.Imported, Is.Empty);
    }

    [Test]
    public async Task Import_WhenNotConfirmed_DoesNotImport()
    {
        (DatabaseImportViewModel vm, FakeDatabaseImporter importer, _, _) = Build(pickedPath: @"C:\x.z21", confirm: false);

        await vm.ImportCommand.ExecuteAsync(null);

        Assert.That(importer.Imported, Is.Empty);
    }
}
