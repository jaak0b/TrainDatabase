namespace TrainDatabase.Core.Ports;

/// <summary>
/// Imports a layout exported from the Roco/Fleischmann Z21 app (a <c>.z21</c> archive)
/// into the application database, replacing the current contents.
/// </summary>
public interface IDatabaseImporter
{
    Task ImportAsync(string z21FilePath);
}
