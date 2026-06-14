namespace TrainDatabase.Core.Ports;

/// <summary>Loads vehicle image bytes by image name (resolved against platform storage).</summary>
public interface IVehicleImageStore
{
    /// <summary>Returns the image bytes for <paramref name="imageName"/>, or <c>null</c> if none.</summary>
    byte[]? TryGetImage(string imageName);
}
