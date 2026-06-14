using TrainDatabase.Infrastructure.Platform;

namespace TrainDatabase.Infrastructure.IntegrationTest.Platform;

[TestFixture]
public class JsonSettingsStoreTests
{
    private string directory = null!;
    private string filePath = null!;

    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "settings-test", Guid.NewGuid().ToString("N"));
        filePath = Path.Combine(directory, "settings.json");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public void Get_ReturnsNull_WhenKeyUnset()
    {
        JsonSettingsStore store = new(filePath);
        Assert.That(store.Get("missing"), Is.Null);
    }

    [Test]
    public void Set_ThenGet_ReturnsValue_AndPersistsAcrossInstances()
    {
        new JsonSettingsStore(filePath).Set("ClientIP", "192.168.0.111");

        JsonSettingsStore reloaded = new(filePath);
        Assert.That(reloaded.Get("ClientIP"), Is.EqualTo("192.168.0.111"));
    }

    [Test]
    public void Set_Null_RemovesKey()
    {
        JsonSettingsStore store = new(filePath);
        store.Set("k", "v");
        store.Set("k", null);

        Assert.That(store.Get("k"), Is.Null);
    }

    [Test]
    public void Constructor_WithCorruptFile_StartsEmpty()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(filePath, "{ this is not valid json");

        JsonSettingsStore store = new(filePath);
        Assert.That(store.Get("anything"), Is.Null);
    }
}
