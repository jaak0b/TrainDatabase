using Autofac;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.UI.Android;

/// <summary>Android <see cref="IAppStorage"/> rooted at the app-private files directory.</summary>
public sealed class AndroidAppStorage(string filesDirectory) : IAppStorage
{
    public string DatabaseFilePath => Path.Combine(filesDirectory, "Data", "Database.sqlite");

    public string VehicleImageDirectory => Path.Combine(filesDirectory, "Data", "VehicleImage");

    public string LogDirectory => Path.Combine(filesDirectory, "Log");

    public string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}

/// <summary>Overrides platform storage with the Android implementation.</summary>
public sealed class AndroidModule(string filesDirectory) : Module
{
    protected override void Load(ContainerBuilder builder) =>
        builder.RegisterInstance(new AndroidAppStorage(filesDirectory)).As<IAppStorage>().SingleInstance();
}
