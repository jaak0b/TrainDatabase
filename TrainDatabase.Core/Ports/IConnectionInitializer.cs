namespace TrainDatabase.Core.Ports;

public interface IConnectionInitializer
{
    Task ConnectAsync();
}
