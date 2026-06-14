using TrainDatabase.Infrastructure.Platform;

namespace TrainDatabase.Infrastructure.IntegrationTest.Platform;

[TestFixture]
public class DesktopAppStorageTests
{
    [Test]
    public void Paths_AreComposedUnderBaseDirectory()
    {
        string baseDir = Path.Combine(Path.GetTempPath(), "as-test");
        DesktopAppStorage storage = new(baseDir);

        Assert.Multiple(() =>
        {
            Assert.That(storage.DatabaseFilePath, Is.EqualTo(Path.Combine(baseDir, "Data", "Database.sqlite")));
            Assert.That(storage.VehicleImageDirectory, Is.EqualTo(Path.Combine(baseDir, "Data", "VehicleImage")));
            Assert.That(storage.LogDirectory, Is.EqualTo(Path.Combine(baseDir, "Log")));
        });
    }

    [Test]
    public void DefaultBaseDirectory_IsUnderApplicationData()
    {
        DesktopAppStorage storage = new();
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        Assert.That(storage.DatabaseFilePath, Does.StartWith(Path.Combine(appData, "TrainDatabase")));
    }

    [Test]
    public void CreateTempDirectory_ReturnsFreshExistingDirectoryEachCall()
    {
        DesktopAppStorage storage = new();

        string first = storage.CreateTempDirectory();
        string second = storage.CreateTempDirectory();

        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(first), Is.True);
                Assert.That(Directory.Exists(second), Is.True);
                Assert.That(first, Is.Not.EqualTo(second));
            });
        }
        finally
        {
            Directory.Delete(first, recursive: true);
            Directory.Delete(second, recursive: true);
        }
    }
}
