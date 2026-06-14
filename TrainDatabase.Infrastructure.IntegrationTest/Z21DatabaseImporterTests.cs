using System.IO.Compression;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using TrainDatabase.Infrastructure.Import;
using TrainDatabase.Infrastructure.Platform;

namespace TrainDatabase.Infrastructure.IntegrationTest;

[TestFixture]
public class Z21DatabaseImporterTests
{
    private string root = null!;

    [SetUp]
    public void SetUp() => root = Path.Combine(Path.GetTempPath(), "TrainDatabase.Import", Guid.NewGuid().ToString("N"));

    [TearDown]
    public void TearDown()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch (IOException) { }
    }

    [Test]
    public async Task ImportAsync_ReadsVehiclesAndFunctionsFromZ21Archive()
    {
        string z21File = CreateZ21Archive();

        using TempDatabase db = new();
        DesktopAppStorage storage = new(Path.Combine(root, "appdata"));
        Z21DatabaseImporter importer = new(db.Context, storage, NullLogger<Z21DatabaseImporter>.Instance);

        await importer.ImportAsync(z21File);

        Assert.Multiple(() =>
        {
            Assert.That(db.Context.Vehicles.Count(), Is.EqualTo(1));
            Assert.That(db.Context.Vehicles.Single().Name, Is.EqualTo("BR 218"));
            Assert.That(db.Context.Vehicles.Single().Address, Is.EqualTo(36));
            Assert.That(db.Context.Functions.Count(), Is.EqualTo(1));
            Assert.That(db.Context.Functions.Single().Name, Is.EqualTo("Light"));
        });
    }

    /// <summary>Builds a minimal .z21 archive (a renamed zip of nested folders + a Roco-schema SQLite db).</summary>
    private string CreateZ21Archive()
    {
        string contentDir = Path.Combine(root, "content");
        string nested = Path.Combine(contentDir, "layer1", "layer2");
        Directory.CreateDirectory(nested);

        string sqlitePath = Path.Combine(nested, "z21.sqlite");
        using (SqliteConnection connection = new($"Data Source={sqlitePath}"))
        {
            connection.Open();
            Execute(connection, """
                CREATE TABLE vehicles (id INTEGER, name TEXT, image_name TEXT, type INTEGER, max_speed INTEGER,
                    address INTEGER, active INTEGER, position INTEGER, full_name TEXT, speed_display INTEGER,
                    railway TEXT, traction_direction INTEGER, description TEXT, dummy INTEGER);
                """);
            Execute(connection, """
                INSERT INTO vehicles VALUES (1,'BR 218','',0,160,36,1,0,'DB BR 218',128,'DB',0,'',0);
                """);
            Execute(connection, """
                CREATE TABLE functions (id INTEGER, vehicle_id INTEGER, button_type INTEGER, shortcut TEXT,
                    time TEXT, position INTEGER, image_name TEXT, function INTEGER, show_function_number INTEGER, is_configured INTEGER);
                """);
            Execute(connection, """
                INSERT INTO functions VALUES (1,1,0,'Light','0',0,'light.png',0,1,1);
                """);
        }
        SqliteConnection.ClearAllPools();

        string zipPath = Path.Combine(root, "layout.zip");
        ZipFile.CreateFromDirectory(contentDir, zipPath);

        string z21Path = Path.Combine(root, "layout.z21");
        File.Move(zipPath, z21Path);
        return z21Path;
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
