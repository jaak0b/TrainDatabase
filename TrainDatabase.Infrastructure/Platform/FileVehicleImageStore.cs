using TrainDatabase.Core.Ports;

namespace TrainDatabase.Infrastructure.Platform;

/// <summary>Reads vehicle images from <see cref="IAppStorage.VehicleImageDirectory"/>.</summary>
public sealed class FileVehicleImageStore(IAppStorage storage) : IVehicleImageStore
{
    public byte[]? TryGetImage(string imageName)
    {
        if (string.IsNullOrWhiteSpace(imageName))
        {
            return null;
        }

        string path = Path.Combine(storage.VehicleImageDirectory, imageName);
        return File.Exists(path) ? File.ReadAllBytes(path) : null;
    }
}
